using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.Loans.DataContext;

/// <summary>
/// Derived DbContext used exclusively for generating and routing SQL Server migrations.
/// </summary>
public class LoansSqlServerDBContext(
    DbContextOptions<LoansSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<LoansDBContext>? logger = null) : LoansDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(LoansSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Loans.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
}
