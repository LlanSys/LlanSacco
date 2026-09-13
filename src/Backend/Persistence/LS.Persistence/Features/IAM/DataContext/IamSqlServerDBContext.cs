using LS.Domain.Shared.Contracts.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.IAM.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class IamSqlServerDBContext(
    DbContextOptions<IamSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<IamDBContext>? logger = null) : IamDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(IamSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.IAM.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
