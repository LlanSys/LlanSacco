using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Contracts.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

namespace LS.Application.Features.Banking.Deposits.Jobs;

public class ProcessDepositMaturitiesJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<ProcessDepositMaturitiesJob> logger)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting ProcessDepositMaturitiesJob");
        
        // Find all active accounts that have reached or passed maturity
        var now = DateTimeOffset.UtcNow;
        var maturedAccounts = await unitOfWork.DepositAccounts.ListAsync(
            q => q.Where(x => x.IsActive && x.MaturityDate <= now),
            cancellationToken);

        foreach (var account in maturedAccounts)
        {
            try
            {
                // Capitalize accrued interest
                if (account.AccruedInterest > 0)
                {
                    account.Balance += account.AccruedInterest;
                    
                    var interestTx = DepositTransaction.Create(
                        account.Id,
                        DepositTransactionType.InterestAccrual,
                        account.AccruedInterest,
                        account.Balance,
                        "Maturity Interest Capitalization",
                        ICurrentActorProvider.SystemActor
                    );
                    
                    account.AccruedInterest = 0;
                    
                    await unitOfWork.DepositTransactions.CreateAsync(interestTx, cancellationToken);
                }
                
                // For a fixed deposit, once matured, we might close it or auto-renew it. 
                // Currently, we just mark it inactive so it can't be deposited into easily, 
                // or we could leave it active and let members withdraw without penalty.
                // We'll leave it active for now but it has passed MaturityDate, so penalties won't apply.
                
                await unitOfWork.DepositAccounts.UpdateAsync(account, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing maturity for account {AccountId}", account.Id);
            }
        }
        
        await unitOfWork.CompleteAsync(cancellationToken);
        
        logger.LogInformation("Finished ProcessDepositMaturitiesJob");
    }
}
