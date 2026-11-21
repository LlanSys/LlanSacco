using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Accounting.DataContext;

namespace LS.Persistence.Features.Accounting.Repositories;

public class TransactionTypeGlMappingRepository(AccountingDBContext context) : Repository<TransactionTypeGlMapping>(context), ITransactionTypeGlMappingRepository;
