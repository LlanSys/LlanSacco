using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.Banking.Deposits.Contracts.Repositories;

public interface IDepositAccountRepository : IRepository<DepositAccount>;
