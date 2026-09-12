using System.Collections.ObjectModel;
using LS.Application.Features.Banking.Logging;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Features.Banking.Savings.Events;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Savings.Jobs;

public class CalculateSavingsInterestJob(
    IBankingUnitOfWork unitOfWork,
    ILogger<CalculateSavingsInterestJob> logger,
    MediatR.IPublisher publisher,
    ICurrentTenantProvider tenantProvider,
    TimeProvider? timeProvider = null)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        var accrualDate = DateOnly.FromDateTime((timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime);
        var processed = 0;
        while (true)
        {
            var count = await unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var products = await unitOfWork.SavingsProducts.ListAsync(q => q.Where(p => p.TenantId == tenantId && p.IsActive && p.InterestRate > 0), cancellationToken);
                var productIds = products.Select(p => p.Id).ToArray();
                var byId = products.ToDictionary(p => p.Id);
                var accounts = await unitOfWork.SavingsAccounts.ListAsync(q => q.Where(a => a.TenantId == tenantId && a.IsActive
                        && productIds.Contains(a.SavingsProductId) && (a.LastInterestAccruedOn == null || a.LastInterestAccruedOn < accrualDate))
                    .OrderBy(a => a.Id).Take(200), cancellationToken);
                foreach (var account in accounts)
                {
                    var product = byId[account.SavingsProductId];
                    var availableBalance = account.GetAvailableBalance(product.MinimumBalance);
                    var interest = Math.Round(Math.Max(0, availableBalance) * (product.InterestRate / 100m) / 365m, 2);
                    account.LastInterestAccruedOn = accrualDate;
                    if (interest <= 0) continue;
                    account.Balance += interest;
                    var transaction = SavingsTransaction.Create(account.Id, SavingsTransactionType.Interest, interest,
                        $"Daily interest for {accrualDate:yyyy-MM-dd}", $"INT:{account.Id:N}:{accrualDate:yyyyMMdd}", ICurrentActorProvider.SystemActor);
                    await unitOfWork.SavingsTransactions.CreateAsync(transaction, cancellationToken);
                    await publisher.Publish(new SavingsInterestAppliedIntegrationEvent(account.MemberId, account.Id, product.Id, interest), cancellationToken);
                }
                await unitOfWork.SavingsAccounts.UpdateRangeAsync(new Collection<SavingsAccount>(accounts), cancellationToken);
                return accounts.Count;
            }, cancellationToken: cancellationToken);
            processed += count;
            if (count < 200) break;
        }
        BankingInterestLogDefinitions.Completed(logger, "Savings accrual", processed);
    }
}
