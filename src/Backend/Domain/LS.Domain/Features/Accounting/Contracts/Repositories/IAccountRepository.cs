using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Domain.Features.Accounting.Contracts.Repositories;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByCodeAsync(string accountCode, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(DateTimeOffset asOfDate, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
}
