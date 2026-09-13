using LS.Application.Features.Banking.Deposits.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Deposits.Handlers;

internal class DepositToAccountCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) 
    : IRequestHandler<DepositToAccountCommand, AppResponse<DepositTransactionResponse>>
{
    public async Task<AppResponse<DepositTransactionResponse>> Handle(DepositToAccountCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var account = await unitOfWork.DepositAccounts.FindByIdAsync(command.AccountId, cancellationToken);
        if (account == null)
        {
            return AppResponses.NotFound<DepositTransactionResponse>($"Deposit account with ID {command.AccountId} not found.");
        }

        if (!account.IsActive)
        {
            return AppResponses.Failure<DepositTransactionResponse>("Cannot deposit into an inactive account.");
        }

        // Only allow deposits if the product allows it. E.g. Fixed deposits might only allow initial deposit.
        // For now, we'll just process the deposit.
        account.Balance += request.Amount;

        var transaction = DepositTransaction.Create(
            account.Id,
            DepositTransactionType.Deposit,
            request.Amount,
            account.Balance,
            request.Reference,
            actorProvider.ActorId.ToString()
        );

        await unitOfWork.DepositTransactions.CreateAsync(transaction, cancellationToken);
        await unitOfWork.DepositAccounts.UpdateAsync(account, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new DepositTransactionResponse(
            transaction.Id,
            transaction.DepositAccountId,
            transaction.Type.ToString(),
            transaction.Amount,
            transaction.BalanceAfter,
            transaction.Reference,
            transaction.CreatedAt
        );

        return AppResponses.Success("Deposit transaction processed successfully.", response);
    }
}

