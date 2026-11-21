using LS.Domain.Features.Dividends.Contracts.Repositories;
using LS.Domain.Features.Dividends.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Dividends.DataContext;

namespace LS.Persistence.Features.Dividends.Repositories;

internal sealed class DividendCalculationRepository(DividendsDBContext context)
    : Repository<DividendCalculation>(context), IDividendCalculationRepository;
