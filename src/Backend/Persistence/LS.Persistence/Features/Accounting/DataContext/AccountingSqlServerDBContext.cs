using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Accounting.DataContext;

public class AccountingSqlServerDBContext(
    DbContextOptions<AccountingSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<AccountingSqlServerDBContext>? logger = null
    ) : AccountingDBContext(options, tenantProvider, actorProvider, logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add SqlServer specific configurations if any
    }
}
