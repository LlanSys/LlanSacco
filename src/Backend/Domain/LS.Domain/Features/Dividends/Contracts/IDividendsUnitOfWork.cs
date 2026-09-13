using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Repositories;

using LS.Domain.Shared.Contracts;

namespace LS.Domain.Features.Dividends.Contracts;

public interface IDividendsUnitOfWork : ITransactionalUnitOfWork
{
    IRepository<DividendDeclaration> DividendDeclarations { get; }
    IRepository<DividendCalculation> DividendCalculations { get; }
    IRepository<DividendDistributionPreference> DividendDistributionPreferences { get; }
}
