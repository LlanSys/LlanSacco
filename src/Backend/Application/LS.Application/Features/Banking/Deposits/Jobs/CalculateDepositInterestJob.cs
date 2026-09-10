using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

namespace LS.Application.Features.Banking.Deposits.Jobs;

public class CalculateDepositInterestJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<CalculateDepositInterestJob> logger)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting CalculateDepositInterestJob");

        var activeAccounts = await unitOfWork.DepositAccounts.ListAsync(
            q => q.Where(x => x.IsActive && x.Balance > 0),
            cancellationToken);

        var productIds = activeAccounts.Select(x => x.DepositProductId).Distinct().ToList();
        var products = await unitOfWork.DepositProducts.ListAsync(
            q => q.Where(x => productIds.Contains(x.Id) && x.InterestRate > 0),
            cancellationToken);
            
        var interestProducts = products.ToDictionary(x => x.Id);

        foreach (var account in activeAccounts)
        {
            try
            {
                if (!interestProducts.TryGetValue(account.DepositProductId, out var product))
                {
                    continue; // Skip if product has no interest rate
                }

                // Daily interest calculation: (Balance * Rate) / 365
                decimal dailyRate = (product.InterestRate / 100m) / 365m;
                decimal dailyInterest = account.Balance * dailyRate;

                // Add to accrued interest
                account.AccruedInterest += dailyInterest;
                
                await unitOfWork.DepositAccounts.UpdateAsync(account, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating interest for deposit account {AccountId}", account.Id);
            }
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation("Finished CalculateDepositInterestJob");
    }
}
