using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Accounting.DataContext;

namespace LS.Persistence.Features.Accounting.Repositories;

public class JournalRepository(AccountingDBContext context) : Repository<Journal>(context), IJournalRepository
{
}
