using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.CheckOff.DataContext;

public class CheckOffSqlServerDBContext(
    DbContextOptions<CheckOffSqlServerDBContext> options,
    ICurrentTenantProvider? tenantProvider = null,
    ICurrentActorProvider? actorProvider = null,
    ILogger<CheckOffDBContext>? logger = null)
    : CheckOffDBContext(options, tenantProvider, actorProvider, logger);
