using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Entities;

using LS.Domain.Features.Banking.Savings.Entities;

namespace LS.Domain.Features.Banking.Contracts;

public interface IBankingUnitOfWork : ITransactionalUnitOfWork
{
    IRepository<ShareProduct> ShareProducts { get; }
    IRepository<ShareAccount> ShareAccounts { get; }
    IRepository<ShareTransaction> ShareTransactions { get; }
    
    IRepository<SavingsProduct> SavingsProducts { get; }
    IRepository<SavingsAccount> SavingsAccounts { get; }
    IRepository<SavingsTransaction> SavingsTransactions { get; }

    IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositProduct> DepositProducts { get; }
    IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositAccount> DepositAccounts { get; }
    IRepository<LS.Domain.Features.Banking.Deposits.Entities.DepositTransaction> DepositTransactions { get; }

    IRepository<LS.Domain.Features.Banking.FOSA.Entities.FosaAccount> FosaAccounts { get; }
    IRepository<LS.Domain.Features.Banking.FOSA.Entities.TellerTill> TellerTills { get; }
    IRepository<LS.Domain.Features.Banking.FOSA.Entities.Vault> Vaults { get; }
    IRepository<LS.Domain.Features.Banking.FOSA.Entities.TillBalancingRecord> TillBalancingRecords { get; }
    IRepository<LS.Domain.Features.Banking.FOSA.Entities.FosaTransaction> FosaTransactions { get; }
}
