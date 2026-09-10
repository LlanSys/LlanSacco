using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Banking;

internal sealed class BankingUnitOfWork(
    BankingDBContext context,
    IPublisher publisher,
    ILogger<BankingUnitOfWork> logger
) : BaseUnitOfWork<BankingDBContext>(context, publisher, logger), IBankingUnitOfWork
{
    public IRepository<ShareProduct> ShareProducts => new Repository<ShareProduct>(Context);
    public IRepository<ShareAccount> ShareAccounts => new Repository<ShareAccount>(Context);
    public IRepository<ShareTransaction> ShareTransactions => new Repository<ShareTransaction>(Context);

    public IRepository<SavingsProduct> SavingsProducts => new Repository<SavingsProduct>(Context);
    public IRepository<SavingsAccount> SavingsAccounts => new Repository<SavingsAccount>(Context);
    public IRepository<SavingsTransaction> SavingsTransactions => new Repository<SavingsTransaction>(Context);

    public IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositProduct> DepositProducts => new Repository<LS.Domain.Features.Banking.Deposits.Entities.DepositProduct>(Context);
    public IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositAccount> DepositAccounts => new Repository<LS.Domain.Features.Banking.Deposits.Entities.DepositAccount>(Context);
    public IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositTransaction> DepositTransactions => new Repository<LS.Domain.Features.Banking.Deposits.Entities.DepositTransaction>(Context);

    public IRepository<LS.Domain.Features.Banking.FOSA.Entities.FosaAccount> FosaAccounts => new Repository<LS.Domain.Features.Banking.FOSA.Entities.FosaAccount>(Context);
    public IRepository<LS.Domain.Features.Banking.FOSA.Entities.TellerTill> TellerTills => new Repository<LS.Domain.Features.Banking.FOSA.Entities.TellerTill>(Context);
    public IRepository<LS.Domain.Features.Banking.FOSA.Entities.Vault> Vaults => new Repository<LS.Domain.Features.Banking.FOSA.Entities.Vault>(Context);
    public IRepository<LS.Domain.Features.Banking.FOSA.Entities.TillBalancingRecord> TillBalancingRecords => new Repository<LS.Domain.Features.Banking.FOSA.Entities.TillBalancingRecord>(Context);
    public IRepository<LS.Domain.Features.Banking.FOSA.Entities.FosaTransaction> FosaTransactions => new Repository<LS.Domain.Features.Banking.FOSA.Entities.FosaTransaction>(Context);
}
