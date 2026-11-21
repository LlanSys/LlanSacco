using LS.Domain.Features.Dividends.Contracts.Repositories;

using LS.Domain.Shared.Contracts;

namespace LS.Domain.Features.Dividends.Contracts;

public interface IDividendsUnitOfWork : ITransactionalUnitOfWork
{
    IDividendDeclarationRepository DividendDeclarationRepository { get; }
    IDividendCalculationRepository DividendCalculationRepository { get; }
    IDividendDistributionPreferenceRepository DividendDistributionPreferenceRepository { get; }
}
