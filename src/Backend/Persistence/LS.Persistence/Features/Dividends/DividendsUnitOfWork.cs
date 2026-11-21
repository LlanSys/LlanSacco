using LS.Domain.Features.Dividends.Contracts.Repositories;
using LS.Domain.Features.Dividends.Contracts;
using LS.Persistence.Common;
using LS.Persistence.Features.Dividends.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Dividends;

public class DividendsUnitOfWork(
    DividendsDBContext context,
    IDividendDeclarationRepository dividendDeclarationRepository,
    IDividendCalculationRepository dividendCalculationRepository,
    IDividendDistributionPreferenceRepository dividendDistributionPreferenceRepository,
    IPublisher publisher,
    ILogger<DividendsUnitOfWork> logger
) : BaseUnitOfWork<DividendsDBContext>(context, publisher, logger), IDividendsUnitOfWork
{
    public IDividendDeclarationRepository DividendDeclarationRepository { get; } = dividendDeclarationRepository;
    public IDividendCalculationRepository DividendCalculationRepository { get; } = dividendCalculationRepository;
    public IDividendDistributionPreferenceRepository DividendDistributionPreferenceRepository { get; } = dividendDistributionPreferenceRepository;
}
