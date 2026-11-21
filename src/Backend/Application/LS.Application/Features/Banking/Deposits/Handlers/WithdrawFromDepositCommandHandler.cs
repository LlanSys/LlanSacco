using LS.Application.Features.Banking.Deposits.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System;
using System.Linq;

namespace LS.Application.Features.Banking.Deposits.Handlers;

internal class WithdrawFromDepositCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) 
    : IRequestHandler<WithdrawFromDepositCommand, AppResponse<DepositTransactionResponse>>
{
    public async Task<AppResponse<DepositTransactionResponse>> Handle(WithdrawFromDepositCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var account = await unitOfWork.DepositAccounts.FindByIdAsync(command.AccountId, cancellationToken);
            
        if (account == null)
        {
            return AppResponses.NotFound<DepositTransactionResponse>($"Deposit account with ID {command.AccountId} not found.");
        }

        if (!account.IsActive)
        {
            return AppResponses.Failure<DepositTransactionResponse>("Cannot withdraw from an inactive account.");
        }

        if (account.Balance < request.Amount)
        {
            return AppResponses.Failure<DepositTransactionResponse>("Insufficient balance for withdrawal.");
        }

        var product = await unitOfWork.DepositProducts.FindByIdAsync(account.DepositProductId, cancellationToken);
        if (product == null)
        {
            return AppResponses.Failure<DepositTransactionResponse>("Deposit product associated with this account not found.");
        }

        // Penalty Engine check
        bool isEarlyWithdrawal = DateTimeOffset.UtcNow < account.MaturityDate;
        
        if (isEarlyWithdrawal)
        {
            var strategy = product.PenaltyStrategy;
            
            if (strategy == EarlyWithdrawalPenaltyStrategy.FlatPercentage && product.FlatPenaltyRate.HasValue)
            {
                // Charge a flat fee on the withdrawn amount
                decimal penaltyAmount = request.Amount * (product.FlatPenaltyRate.Value / 100m);
                // Reject before mutating the tracked account or staging a penalty.
                if (account.Balance - penaltyAmount < request.Amount)
                {
                    return AppResponses.Failure<DepositTransactionResponse>("Insufficient balance for withdrawal after early withdrawal penalties.");
                }
                account.Balance -= penaltyAmount;
                
                var penaltyTx = DepositTransaction.Create(
                    account.Id,
                    DepositTransactionType.Penalty,
                    penaltyAmount,
                    account.Balance,
                    "Early Withdrawal Flat Penalty",
                    actorProvider.ActorId.ToString()
                );
                await unitOfWork.DepositTransactions.CreateAsync(penaltyTx, cancellationToken);
            }
            else if (strategy == EarlyWithdrawalPenaltyStrategy.InterestForfeiture && product.InterestForfeiturePercentage.HasValue)
            {
                // Forfeit a percentage of the accrued interest
                decimal forfeitedAmount = account.AccruedInterest * (product.InterestForfeiturePercentage.Value / 100m);
                account.AccruedInterest -= forfeitedAmount;
                
                // Track forfeiture as a zero-balance-impact penalty transaction for audit purposes, or skip
                var penaltyTx = DepositTransaction.Create(
                    account.Id,
                    DepositTransactionType.Penalty,
                    0, // Principal balance doesn't change
                    account.Balance,
                    $"Early Withdrawal Interest Forfeiture: {forfeitedAmount}",
                    actorProvider.ActorId.ToString()
                );
                await unitOfWork.DepositTransactions.CreateAsync(penaltyTx, cancellationToken);
            }
            else if (strategy == EarlyWithdrawalPenaltyStrategy.ProRataInterestReduction && product.ProRataReducedInterestRate.HasValue)
            {
                // Recalculate interest. For simplicity in this engine, we reduce the accrued interest proportionally
                if (product.InterestRate > 0)
                {
                    decimal ratio = product.ProRataReducedInterestRate.Value / product.InterestRate;
                    decimal newAccruedInterest = account.AccruedInterest * ratio;
                    decimal forfeitedAmount = account.AccruedInterest - newAccruedInterest;
                    
                    account.AccruedInterest = newAccruedInterest;
                    
                    var penaltyTx = DepositTransaction.Create(
                        account.Id,
                        DepositTransactionType.Penalty,
                        0,
                        account.Balance,
                        $"Early Withdrawal ProRata Interest Reduction: {forfeitedAmount}",
                        actorProvider.ActorId.ToString()
                    );
                    await unitOfWork.DepositTransactions.CreateAsync(penaltyTx, cancellationToken);
                }
            }
        }

        // All balance checks have passed; stage the withdrawal with its penalty.
        
        account.Balance -= request.Amount;

        var transaction = DepositTransaction.Create(
            account.Id,
            DepositTransactionType.Withdrawal,
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

        return AppResponses.Success("Withdrawal processed successfully.", response);
    }
}

