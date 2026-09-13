using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Shared.Contracts;

namespace LS.Domain.Features.Accounting.Contracts;

public interface IAccountingUnitOfWork : ITransactionalUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IJournalRepository JournalRepository { get; }
    ITransactionTypeGlMappingRepository TransactionTypeGlMappingRepository { get; }
    IAccountingIntegrationErrorRepository AccountingIntegrationErrorRepository { get; }
}
