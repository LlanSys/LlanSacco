using LS.Domain.Shared.Contracts.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Shared.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class SharedPostgreSqlDBContext(
    DbContextOptions<SharedPostgreSqlDBContext> options,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    ILogger<SharedDBContext>? logger = null) : SharedDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SharedPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Shared.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
