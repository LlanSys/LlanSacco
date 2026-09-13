using LS.Domain.Features.Dividends.Contracts;
using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Dividends.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Dividends.Contracts;

public class DividendsUnitOfWork(
    DividendsDBContext context,
    IPublisher publisher,
    ILogger<DividendsUnitOfWork> logger
) : BaseUnitOfWork<DividendsDBContext>(context, publisher, logger), IDividendsUnitOfWork
{
    public IRepository<DividendDeclaration> DividendDeclarations => new Repository<DividendDeclaration>(Context);
    public IRepository<DividendCalculation> DividendCalculations => new Repository<DividendCalculation>(Context);
    public IRepository<DividendDistributionPreference> DividendDistributionPreferences => new Repository<DividendDistributionPreference>(Context);
}
