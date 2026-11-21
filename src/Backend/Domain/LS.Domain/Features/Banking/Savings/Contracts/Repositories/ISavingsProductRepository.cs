using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.Banking.Savings.Contracts.Repositories;

public interface ISavingsProductRepository : IRepository<SavingsProduct>;
