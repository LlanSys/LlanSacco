using System.Collections.ObjectModel;
using LS.Application.Features.Banking.Logging;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Shared.Contracts.Common;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Deposits.Jobs;

public class CalculateDepositInterestJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<CalculateDepositInterestJob> logger,
    ICurrentTenantProvider tenantProvider,
    TimeProvider? timeProvider = null)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow();
        var accrualDate = DateOnly.FromDateTime(now.UtcDateTime);
        var processed = 0;
        while (true)
        {
            var count = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var products = await unitOfWork.DepositProducts.ListAsync(q => q.Where(p => p.TenantId == tenantId && p.IsActive && p.InterestRate > 0), cancellationToken);
                var productIds = products.Select(p => p.Id).ToArray();
                var byId = products.ToDictionary(p => p.Id);
                var accounts = await unitOfWork.DepositAccounts.ListAsync(q => q.Where(a => a.TenantId == tenantId && a.IsActive && a.Balance > 0
                        && a.MaturityDate > now && productIds.Contains(a.DepositProductId)
                        && (a.LastInterestAccruedOn == null || a.LastInterestAccruedOn < accrualDate))
                    .OrderBy(a => a.Id).Take(200), cancellationToken);
                foreach (var account in accounts)
                {
                    var product = byId[account.DepositProductId];
                    account.AccruedInterest += Math.Round(account.Balance * (product.InterestRate / 100m) / 365m, 4);
                    account.LastInterestAccruedOn = accrualDate;
                }
                await unitOfWork.DepositAccounts.UpdateRangeAsync(new Collection<DepositAccount>(accounts), cancellationToken);
                return accounts.Count;
            }, cancellationToken: cancellationToken);
            processed += count;
            if (count < 200) break;
        }
        BankingInterestLogDefinitions.Completed(logger, "Deposit accrual", processed);
    }
}
