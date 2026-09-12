using Microsoft.EntityFrameworkCore;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Persistence.Features.Shared.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class SharedSqlServerDBContext(
    DbContextOptions<SharedSqlServerDBContext> options,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider,
    Microsoft.Extensions.Logging.ILogger<SharedDBContext>? logger = null) : SharedDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SharedSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Shared.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}

