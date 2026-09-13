using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.Loans.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing PostgreSQL migrations.
/// </summary>
public class LoansPostgreSqlDBContext(
    DbContextOptions<LoansPostgreSqlDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<LoansDBContext>? logger = null) : LoansDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LoansPostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Loans.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
}
