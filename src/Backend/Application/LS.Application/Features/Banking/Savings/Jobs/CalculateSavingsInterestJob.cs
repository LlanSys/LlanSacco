using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.SharedKernel.Features.Banking.Savings.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Jobs;

public class CalculateSavingsInterestJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<CalculateSavingsInterestJob> logger,
    MediatR.IPublisher publisher)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Savings Interest Accrual Job...");

        var activeProducts = await unitOfWork.SavingsProducts.ListAsync(q => q.Where(x => x.IsActive && x.InterestRate > 0), cancellationToken);

        if (!activeProducts.Any())
        {
            logger.LogInformation("No active savings products with an interest rate found. Exiting job.");
            return;
        }

        int processedAccounts = 0;

        foreach (var product in activeProducts)
        {
            // Daily interest calculation = (InterestRate / 100) / 365
            decimal dailyRate = (product.InterestRate / 100m) / 365m;

            var accounts = await unitOfWork.SavingsAccounts.ListAsync(q => q.Where(x => x.SavingsProductId == product.Id && x.IsActive), cancellationToken);

            foreach (var account in accounts)
            {
                decimal availableBalance = account.GetAvailableBalance(product.MinimumBalance);
                
                if (availableBalance <= 0)
                {
                    continue; // No interest for zero or negative available balance
                }

                decimal interestToApply = Math.Round(availableBalance * dailyRate, 2);

                if (interestToApply > 0)
                {
                    account.Balance += interestToApply;

                    var transaction = SavingsTransaction.Create(
                        account.Id,
                        SavingsTransactionType.Interest,
                        interestToApply,
                        $"Daily Interest Accrual at {product.InterestRate}% p.a.",
                        null,
                        "System"
                    );

                    await unitOfWork.SavingsTransactions.CreateAsync(transaction, cancellationToken);
                    
                    await publisher.Publish(new SavingsInterestAppliedIntegrationEvent(
                        account.MemberId,
                        account.Id,
                        product.Id,
                        interestToApply
                    ), cancellationToken);

                    processedAccounts++;
                }
            }
        }

        if (processedAccounts > 0)
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Successfully applied interest to {Count} savings accounts.", processedAccounts);
        }
        else
        {
            logger.LogInformation("No savings accounts were eligible for interest today.");
        }
    }
}
