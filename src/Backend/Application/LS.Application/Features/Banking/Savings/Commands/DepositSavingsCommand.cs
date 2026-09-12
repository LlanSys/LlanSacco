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
    IPublisher publisher) 
    : IRequestHandler<DepositSavingsCommand, AppResponse<SavingsAccountResponse>>
{
    public async Task<AppResponse<SavingsAccountResponse>> Handle(DepositSavingsCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check if external reference was already processed
        if (!string.IsNullOrWhiteSpace(request.ExternalReferenceId))
        {
            bool alreadyProcessed = await unitOfWork.SavingsTransactions.AnyAsync(x => x.ExternalReferenceId == request.ExternalReferenceId && x.Type == SavingsTransactionType.Deposit, cancellationToken);
            
            if (alreadyProcessed)
            {
                return AppResponses.Failure<SavingsAccountResponse>($"A deposit with the external reference ID '{request.ExternalReferenceId}' has already been processed.");
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

        var account = await unitOfWork.SavingsAccounts.FirstOrDefaultAsync(x => x.MemberId == request.MemberId && x.SavingsProductId == request.SavingsProductId, cancellationToken);

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
        await publisher.Publish(new SavingsDepositedIntegrationEvent(
            request.MemberId,
            account.Id,
            product.Id,
            request.Amount,
            request.ExternalReferenceId ?? string.Empty
        ), cancellationToken);

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

