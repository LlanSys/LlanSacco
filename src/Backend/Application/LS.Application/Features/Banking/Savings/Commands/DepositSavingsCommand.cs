using LS.Application.Contracts.Interfaces.Common;
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
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Commands;

public record DepositSavingsCommand(DepositSavingsRequest Request) : IRequest<AppResponse<SavingsAccountResponse>>;

internal class DepositSavingsCommandValidator : AbstractValidator<DepositSavingsCommand>
{
    public DepositSavingsCommandValidator()
    {
        RuleFor(x => x.Request.MemberId).NotEmpty();
        RuleFor(x => x.Request.SavingsProductId).NotEmpty();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Notes).MaximumLength(500);
        RuleFor(x => x.Request.ExternalReferenceId).MaximumLength(100);
    }
}

internal class DepositSavingsCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider,
    IContextEventPublisher<IBankingUnitOfWork> publisher, ICurrentTenantProvider tenantProvider)
    : IRequestHandler<DepositSavingsCommand, AppResponse<SavingsAccountResponse>>
{
    public async Task<AppResponse<SavingsAccountResponse>> Handle(DepositSavingsCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) return AppResponses.Failure<SavingsAccountResponse>(AppError.Forbidden("A tenant is required."));
        if (request.Amount <= 0) return AppResponses.Failure<SavingsAccountResponse>("Deposit amount must be positive.");
        if (!string.IsNullOrWhiteSpace(request.ExternalReferenceId))
        {
            var prior = await unitOfWork.SavingsTransactions.FirstOrDefaultAsync(t => t.TenantId == tenantId
                && t.ExternalReferenceId == request.ExternalReferenceId && t.Type == SavingsTransactionType.Deposit, cancellationToken);
            if (prior is not null)
            {
                var priorAccount = await unitOfWork.SavingsAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == prior.SavingsAccountId, cancellationToken);
                if (priorAccount is null || priorAccount.MemberId != request.MemberId || priorAccount.SavingsProductId != request.SavingsProductId || prior.Amount != request.Amount)
                    return AppResponses.Failure<SavingsAccountResponse>("This payment reference is already associated with different deposit details.");
                var priorProduct = await unitOfWork.SavingsProducts.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == priorAccount.SavingsProductId, cancellationToken);
                if (priorProduct is null) return AppResponses.NotFound<SavingsAccountResponse>("Savings product not found.");
                return AppResponses.Success("Deposit already processed.", new SavingsAccountResponse(priorAccount.Id, priorAccount.MemberId,
                    priorAccount.SavingsProductId, priorProduct.Name, priorAccount.Balance, priorAccount.LockedFunds,
                    priorAccount.GetAvailableBalance(priorProduct.MinimumBalance), priorAccount.IsActive, priorAccount.CreatedAt));
            }
        }

        var product = await unitOfWork.SavingsProducts.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.SavingsProductId, cancellationToken);

        if (product == null)
        {
            return AppResponses.NotFound<SavingsAccountResponse>($"Savings product '{request.SavingsProductId}' not found.");
        }

        if (!product.IsActive)
        {
            return AppResponses.Failure<SavingsAccountResponse>($"Savings product '{product.Name}' is not active.");
        }

        var account = await unitOfWork.SavingsAccounts.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MemberId == request.MemberId && x.SavingsProductId == request.SavingsProductId, cancellationToken);

        bool isNewAccount = account == null;
        if (account == null)
        {
            // Open a new savings account automatically on first deposit
            account = SavingsAccount.Create(request.MemberId, request.SavingsProductId, actorProvider.ActorId.ToString());
            await unitOfWork.SavingsAccounts.CreateAsync(account, cancellationToken);
            // The domain factory assigns the ID; account and transaction commit together.
        }

        if (!account.IsActive)
        {
            return AppResponses.Failure<SavingsAccountResponse>("Savings account is inactive and cannot accept deposits.");
        }

        // Create the transaction
        var transaction = SavingsTransaction.Create(
            account.Id,
            SavingsTransactionType.Deposit,
            request.Amount,
            request.Notes,
            request.ExternalReferenceId,
            actorProvider.ActorId.ToString()
        );

        await unitOfWork.SavingsTransactions.CreateAsync(transaction, cancellationToken);

        // Update balance
        account.Balance += request.Amount;
        if (!isNewAccount)
            await unitOfWork.SavingsAccounts.UpdateAsync(account, cancellationToken);

        // Publish event for Accounting
        await publisher.PublishAsync(new SavingsDepositedIntegrationEvent(
            request.MemberId,
            account.Id,
            product.Id,
            request.Amount,
            request.ExternalReferenceId ?? string.Empty
        ) { TenantId = tenantId, TransactionId = transaction.Id, OccurredAt = transaction.CreatedAt }, cancellationToken);

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

        return AppResponses.Success<SavingsAccountResponse>($"Successfully deposited {request.Amount:C} to {product.Name}.", response);
    }
}

