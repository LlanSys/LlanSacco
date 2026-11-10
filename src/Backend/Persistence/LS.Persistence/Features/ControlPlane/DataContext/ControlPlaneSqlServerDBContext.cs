using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.ControlPlane.DataContext;

public class ControlPlaneSqlServerDBContext(
    DbContextOptions<ControlPlaneSqlServerDBContext> options,
    ICurrentActorProvider? actorProvider = null,
    ILogger<ControlPlaneSqlServerDBContext>? logger = null) : ControlPlaneDBContext(options, actorProvider ?? new SystemActorProvider(), logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ControlPlaneSqlServerDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.ControlPlane.EntityConfigurations.SqlServer", StringComparison.Ordinal) == true);
    }
    
    private sealed class SystemActorProvider : ICurrentActorProvider
    {
        public string ActorId => ICurrentActorProvider.SystemActor;
    }
}
