using System.Collections.ObjectModel;
using LS.Application.Features.Banking.Logging;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Contracts.Common;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Deposits.Jobs;

public class ProcessDepositMaturitiesJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<ProcessDepositMaturitiesJob> logger,
    ICurrentTenantProvider tenantProvider,
    TimeProvider? timeProvider = null)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow();
        var processed = 0;
        while (true)
        {
            var count = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var accounts = await unitOfWork.DepositAccounts.ListAsync(q => q.Where(a => a.TenantId == tenantId && a.IsActive
                        && a.MaturityDate <= now && a.AccruedInterest > 0)
                    .OrderBy(a => a.Id).Take(200), cancellationToken);
                foreach (var account in accounts)
                {
                    var interest = account.AccruedInterest;
                    account.Balance += interest;
                    account.AccruedInterest = 0;
                    await unitOfWork.DepositTransactions.CreateAsync(DepositTransaction.Create(account.Id,
                        DepositTransactionType.InterestAccrual, interest, account.Balance, "Maturity Interest Capitalization",
                        ICurrentActorProvider.SystemActor), cancellationToken);
                }
                await unitOfWork.DepositAccounts.UpdateRangeAsync(new Collection<DepositAccount>(accounts), cancellationToken);
                return accounts.Count;
            }, cancellationToken: cancellationToken);
            processed += count;
            if (count < 200) break;
        }
        BankingInterestLogDefinitions.Completed(logger, "Deposit maturity", processed);
    }
}
