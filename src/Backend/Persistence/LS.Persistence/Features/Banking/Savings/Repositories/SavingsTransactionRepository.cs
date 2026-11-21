using LS.Domain.Features.Banking.Savings.Contracts.Repositories;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.Savings.Repositories;

internal sealed class SavingsTransactionRepository(BankingDBContext context)
    : Repository<SavingsTransaction>(context), ISavingsTransactionRepository;
