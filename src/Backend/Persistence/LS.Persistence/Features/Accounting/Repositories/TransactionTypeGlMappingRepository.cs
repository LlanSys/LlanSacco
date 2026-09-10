using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Accounting.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Persistence.Features.Accounting.Repositories;

public class TransactionTypeGlMappingRepository(AccountingDBContext context) : Repository<TransactionTypeGlMapping>(context), ITransactionTypeGlMappingRepository
{
    private readonly AccountingDBContext _context = context;

    public async Task<TransactionTypeGlMapping?> GetByTransactionTypeCodeAsync(string transactionTypeCode, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(m => m.TransactionTypeCode == transactionTypeCode, cancellationToken).ConfigureAwait(false);
    }
}
