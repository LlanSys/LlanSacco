using LS.Domain.Features.Banking.FOSA.Contracts.Repositories;
using LS.Domain.Features.Banking.Deposits.Contracts.Repositories;
using LS.Domain.Features.Banking.Savings.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Contracts.Repositories;
using LS.Domain.Shared.Contracts;


namespace LS.Domain.Features.Banking.Contracts;

public interface IBankingUnitOfWork : ITransactionalUnitOfWork
{
    IShareProductRepository ShareProductRepository { get; }
    IShareAccountRepository ShareAccountRepository { get; }
    IShareTransactionRepository ShareTransactionRepository { get; }
    
    ISavingsProductRepository SavingsProductRepository { get; }
    ISavingsAccountRepository SavingsAccountRepository { get; }
    ISavingsTransactionRepository SavingsTransactionRepository { get; }

    IDepositProductRepository DepositProductRepository { get; }
    IDepositAccountRepository DepositAccountRepository { get; }
    IDepositTransactionRepository DepositTransactionRepository { get; }

    IFosaAccountRepository FosaAccountRepository { get; }
    ITellerTillRepository TellerTillRepository { get; }
    IVaultRepository VaultRepository { get; }
    ITillBalancingRecordRepository TillBalancingRecordRepository { get; }
    IFosaTransactionRepository FosaTransactionRepository { get; }
    IVaultTransferRepository VaultTransferRepository { get; }
}
