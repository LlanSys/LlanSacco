using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Dividends.Providers.SqlServer;

public class DividendsSqlServerDBContext(
    DbContextOptions<DividendsSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<DataContext.DividendsDBContext>? logger = null) 
    : DataContext.DividendsDBContext(options, tenantProvider, actorProvider, logger)
{
}
