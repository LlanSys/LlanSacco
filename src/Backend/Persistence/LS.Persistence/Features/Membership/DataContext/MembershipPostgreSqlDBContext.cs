using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.Membership.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class MembershipPostgreSqlDBContext(
    DbContextOptions<MembershipPostgreSqlDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<MembershipDBContext>? logger = null) : MembershipDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MembershipPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Membership.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
