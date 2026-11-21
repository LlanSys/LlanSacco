using LS.Domain.Shared.Contracts.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.IAM.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class IamPostgreSqlDBContext(
    DbContextOptions<IamPostgreSqlDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<IamDBContext>? logger = null) : IamDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(IamPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.IAM.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
