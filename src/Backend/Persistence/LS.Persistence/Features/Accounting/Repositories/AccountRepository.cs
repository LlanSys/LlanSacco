using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Accounting.DataContext;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.Accounting.Repositories;

public class AccountRepository(AccountingDBContext context) : Repository<Account>(context), IAccountRepository
{
    private readonly AccountingDBContext _context = context;

    public async Task<Account?> GetByCodeAsync(string accountCode, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(a => a.AccountCode == accountCode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(DateTimeOffset asOfDate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JournalLine>()
            .Where(jl => jl.Journal.TransactionDate <= asOfDate)
            .GroupBy(jl => jl.AccountId)
            .Select(g => new { AccountId = g.Key, Balance = g.Sum(jl => jl.Debit - jl.Credit) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Balance, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<JournalLine>()
            .Where(jl => jl.Journal.TransactionDate >= startDate && jl.Journal.TransactionDate <= endDate)
            .GroupBy(jl => jl.AccountId)
            .Select(g => new { AccountId = g.Key, Balance = g.Sum(jl => jl.Debit - jl.Credit) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Balance, cancellationToken).ConfigureAwait(false);
    }
}
