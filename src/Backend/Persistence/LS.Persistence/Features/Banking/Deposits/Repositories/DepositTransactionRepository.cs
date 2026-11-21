using LS.Domain.Features.Banking.Deposits.Contracts.Repositories;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.Deposits.Repositories;

internal sealed class DepositTransactionRepository(BankingDBContext context)
    : Repository<DepositTransaction>(context), IDepositTransactionRepository;
