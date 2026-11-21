using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Dividends.Providers.PostgreSql;

public class DividendsPostgreSqlDBContext(
    DbContextOptions<DividendsPostgreSqlDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<DataContext.DividendsDBContext>? logger = null) 
    : DataContext.DividendsDBContext(options, tenantProvider, actorProvider, logger)
{
}
