using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.Accounting.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Accounting.Repositories;

public class AccountingUnitOfWork(AccountingDBContext context,
    IAccountRepository accountRepository,
    IJournalRepository journalRepository,
    ITransactionTypeGlMappingRepository transactionTypeGlMappingRepository,
    IAccountingIntegrationErrorRepository accountingIntegrationErrorRepository,
    IPublisher publisher,
    ILogger<AccountingUnitOfWork> logger) : BaseUnitOfWork<AccountingDBContext>(context, publisher, logger), IAccountingUnitOfWork
{
    public IAccountRepository AccountRepository { get; } = accountRepository;
    public IJournalRepository JournalRepository { get; } = journalRepository;
    public ITransactionTypeGlMappingRepository TransactionTypeGlMappingRepository { get; } = transactionTypeGlMappingRepository;
    public IAccountingIntegrationErrorRepository AccountingIntegrationErrorRepository { get; } = accountingIntegrationErrorRepository;
}
