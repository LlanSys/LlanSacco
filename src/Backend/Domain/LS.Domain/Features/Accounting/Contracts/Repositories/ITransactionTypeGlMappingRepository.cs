using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Domain.Features.Accounting.Contracts.Repositories;

public interface ITransactionTypeGlMappingRepository : IRepository<TransactionTypeGlMapping>
{
    Task<TransactionTypeGlMapping?> GetByTransactionTypeCodeAsync(string transactionTypeCode, CancellationToken cancellationToken = default);
}
