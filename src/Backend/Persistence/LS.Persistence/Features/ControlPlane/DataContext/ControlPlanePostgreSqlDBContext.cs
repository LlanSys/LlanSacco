using LS.Domain.Shared.Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LS.Persistence.Features.ControlPlane.DataContext;

public class ControlPlanePostgreSqlDBContext(
    DbContextOptions<ControlPlanePostgreSqlDBContext> options,
    ICurrentActorProvider? actorProvider = null,
    ILogger<ControlPlanePostgreSqlDBContext>? logger = null) : ControlPlaneDBContext(options, actorProvider ?? new SystemActorProvider(), logger)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ControlPlanePostgreSqlDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.ControlPlane.EntityConfigurations.PostgreSql", StringComparison.Ordinal) == true);
    }
    
    private sealed class SystemActorProvider : ICurrentActorProvider
    {
        public string ActorId => ICurrentActorProvider.SystemActor;
    }
}
