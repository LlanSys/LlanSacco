using LS.Domain.Features.Banking.FOSA.Contracts.Repositories;
using LS.Domain.Features.Banking.Deposits.Contracts.Repositories;
using LS.Domain.Features.Banking.Savings.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Contracts.Repositories;
using LS.Domain.Features.Banking.Contracts;
using LS.Persistence.Common;
using LS.Persistence.Features.Banking.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Banking;

internal sealed class BankingUnitOfWork(
    BankingDBContext context,
    IVaultTransferRepository vaultTransferRepository,
    IShareProductRepository shareProductRepository,
    IShareAccountRepository shareAccountRepository,
    IShareTransactionRepository shareTransactionRepository,
    ISavingsProductRepository savingsProductRepository,
    ISavingsAccountRepository savingsAccountRepository,
    ISavingsTransactionRepository savingsTransactionRepository,
    IDepositProductRepository depositProductRepository,
    IDepositAccountRepository depositAccountRepository,
    IDepositTransactionRepository depositTransactionRepository,
    IFosaAccountRepository fosaAccountRepository,
    IFosaTransactionRepository fosaTransactionRepository,
    ITellerTillRepository tellerTillRepository,
    ITillBalancingRecordRepository tillBalancingRecordRepository,
    IVaultRepository vaultRepository,
    IPublisher publisher,
    ILogger<BankingUnitOfWork> logger
) : BaseUnitOfWork<BankingDBContext>(context, publisher, logger), IBankingUnitOfWork
{
    public IVaultTransferRepository VaultTransferRepository { get; } = vaultTransferRepository;

    public IShareProductRepository ShareProductRepository { get; } = shareProductRepository;
    public IShareAccountRepository ShareAccountRepository { get; } = shareAccountRepository;
    public IShareTransactionRepository ShareTransactionRepository { get; } = shareTransactionRepository;

    public ISavingsProductRepository SavingsProductRepository { get; } = savingsProductRepository;
    public ISavingsAccountRepository SavingsAccountRepository { get; } = savingsAccountRepository;
    public ISavingsTransactionRepository SavingsTransactionRepository { get; } = savingsTransactionRepository;

    public IDepositProductRepository DepositProductRepository { get; } = depositProductRepository;
    public IDepositAccountRepository DepositAccountRepository { get; } = depositAccountRepository;
    public IDepositTransactionRepository DepositTransactionRepository { get; } = depositTransactionRepository;

    public IFosaAccountRepository FosaAccountRepository { get; } = fosaAccountRepository;
    public ITellerTillRepository TellerTillRepository { get; } = tellerTillRepository;
    public IVaultRepository VaultRepository { get; } = vaultRepository;
    public ITillBalancingRecordRepository TillBalancingRecordRepository { get; } = tillBalancingRecordRepository;
    public IFosaTransactionRepository FosaTransactionRepository { get; } = fosaTransactionRepository;
}
