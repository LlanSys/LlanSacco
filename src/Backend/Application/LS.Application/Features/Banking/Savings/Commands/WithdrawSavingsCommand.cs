using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.SharedKernel.Features.Banking.Savings.Events;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Commands;

public record WithdrawSavingsCommand(WithdrawSavingsRequest Request) : IRequest<AppResponse<SavingsAccountResponse>>;

internal class WithdrawSavingsCommandValidator : AbstractValidator<WithdrawSavingsCommand>
{
    public WithdrawSavingsCommandValidator()
    {
        RuleFor(x => x.Request.MemberId).NotEmpty();
        RuleFor(x => x.Request.SavingsProductId).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Notes).MaximumLength(500);
        RuleFor(x => x.Request.ExternalReferenceId).MaximumLength(100);
    }
}

internal class WithdrawSavingsCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    IPublisher publisher) 
    : IRequestHandler<WithdrawSavingsCommand, AppResponse<SavingsAccountResponse>>
{
    public async Task<AppResponse<SavingsAccountResponse>> Handle(WithdrawSavingsCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Note on concurrency: we will use EF Core's optimistic concurrency (RowVersion) to prevent race conditions during updates.
        
        if (!string.IsNullOrWhiteSpace(request.ExternalReferenceId))
        {
            bool alreadyProcessed = await unitOfWork.SavingsTransactions.AnyAsync(x => x.ExternalReferenceId == request.ExternalReferenceId && x.Type == SavingsTransactionType.Withdrawal, cancellationToken);
            
            if (alreadyProcessed)
            {
                return AppResponses.Failure<SavingsAccountResponse>($"A withdrawal with the external reference ID '{request.ExternalReferenceId}' has already been processed.");
            }
        }

        var product = await unitOfWork.SavingsProducts.FirstOrDefaultAsync(x => x.Id == request.SavingsProductId, cancellationToken);

        if (product == null)
        {
            return AppResponses.NotFound<SavingsAccountResponse>($"Savings product '{request.SavingsProductId}' not found.");
        }

        if (!product.IsActive)
        {
            return AppResponses.Failure<SavingsAccountResponse>($"Savings product '{product.Name}' is not active.");
        }
        
        if (!product.AllowsWithdrawals)
        {
            return AppResponses.Failure<SavingsAccountResponse>($"Savings product '{product.Name}' does not allow withdrawals.");
        }

        var account = await unitOfWork.SavingsAccounts.FirstOrDefaultAsync(x => x.MemberId == request.MemberId && x.SavingsProductId == request.SavingsProductId, cancellationToken);

        if (account == null)
        {
            return AppResponses.NotFound<SavingsAccountResponse>($"No savings account found for this member and product.");
        }

        if (!account.IsActive)
        {
            return AppResponses.Failure<SavingsAccountResponse>("Savings account is inactive and cannot perform withdrawals.");
        }

        // Limit Checks
        if (product.DailyWithdrawalLimit.HasValue || product.MonthlyWithdrawalLimit.HasValue)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var startOfMonth = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var recentWithdrawals = await unitOfWork.SavingsTransactions.ListAsync(q => q
                .Where(x => x.SavingsAccountId == account.Id 
                            && x.Type == SavingsTransactionType.Withdrawal 
                            && x.CreatedAt >= startOfMonth), cancellationToken);

            if (product.DailyWithdrawalLimit.HasValue)
            {
                var todayTotal = recentWithdrawals
                    .Where(x => x.CreatedAt.Date == today)
                    .Sum(x => x.Amount);
                if (todayTotal + request.Amount > product.DailyWithdrawalLimit.Value)
                {
                    return AppResponses.Failure<SavingsAccountResponse>($"Withdrawal exceeds the daily limit of {product.DailyWithdrawalLimit.Value:C}.");
                }
            }
            
            if (product.MonthlyWithdrawalLimit.HasValue)
            {
                var monthTotal = recentWithdrawals.Sum(x => x.Amount);
                if (monthTotal + request.Amount > product.MonthlyWithdrawalLimit.Value)
                {
                    return AppResponses.Failure<SavingsAccountResponse>($"Withdrawal exceeds the monthly limit of {product.MonthlyWithdrawalLimit.Value:C}.");
                }
            }
        }

        decimal totalDeduction = request.Amount + product.WithdrawalFee;
        decimal availableBalance = account.GetAvailableBalance(product.MinimumBalance);

        if (availableBalance < totalDeduction)
        {
            return AppResponses.Failure<SavingsAccountResponse>(
                $"Insufficient available balance. You requested {request.Amount:C} + {product.WithdrawalFee:C} fee. Available: {availableBalance:C}");
        }

        // Update balance
        account.Balance -= totalDeduction;
        await unitOfWork.SavingsAccounts.UpdateAsync(account, cancellationToken);

        // Record withdrawal transaction
        var withdrawalTransaction = SavingsTransaction.Create(
            account.Id,
            SavingsTransactionType.Withdrawal,
            request.Amount,
            request.Notes,
            request.ExternalReferenceId,
            actorProvider.ActorId.ToString()
        );
        await unitOfWork.SavingsTransactions.CreateAsync(withdrawalTransaction, cancellationToken);

        // Record fee transaction if applicable
        if (product.WithdrawalFee > 0)
        {
            var feeTransaction = SavingsTransaction.Create(
                account.Id,
                SavingsTransactionType.Fee,
                product.WithdrawalFee,
                "Withdrawal Fee",
                request.ExternalReferenceId,
                actorProvider.ActorId.ToString()
            );
            await unitOfWork.SavingsTransactions.CreateAsync(feeTransaction, cancellationToken);
        }

        // Publish event for Accounting
        await publisher.Publish(new SavingsWithdrawnIntegrationEvent(
            request.MemberId,
            account.Id,
            product.Id,
            request.Amount,
            request.ExternalReferenceId ?? string.Empty,
            product.WithdrawalFee
        ), cancellationToken);

        // Let EF handle optimistic concurrency on SaveChangesAsync. If RowVersion changed, it will throw DbUpdateConcurrencyException
        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new SavingsAccountResponse(
            account.Id,
            account.MemberId,
            account.SavingsProductId,
            product.Name,
            account.Balance,
            account.LockedFunds,
            account.GetAvailableBalance(product.MinimumBalance),
            account.IsActive,
            account.CreatedAt
        );

        return AppResponses.Success<SavingsAccountResponse>($"Successfully withdrew {request.Amount:C} from {product.Name}.", response);
    }
}

