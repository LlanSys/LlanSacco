using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.Banking.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class BankingSqlServerDBContext(
    DbContextOptions<BankingSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<BankingDBContext>? logger = null) : BankingDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BankingSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Banking.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
