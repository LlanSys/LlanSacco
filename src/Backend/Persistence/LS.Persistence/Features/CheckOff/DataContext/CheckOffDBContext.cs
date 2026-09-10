using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Shared.Entities;
using LS.Persistence.Common;
using LS.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Persistence.Features.CheckOff.DataContext;

public class CheckOffDBContext : DbContext, ITenantFilteredDBContext
{
    private readonly ICurrentTenantProvider? _tenantProvider;
    private readonly ICurrentActorProvider? _actorProvider;
    private readonly ILogger<CheckOffDBContext>? _logger;

    public CheckOffDBContext(
        DbContextOptions<CheckOffDBContext> options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<CheckOffDBContext>? logger = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    protected CheckOffDBContext(
        DbContextOptions options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<CheckOffDBContext>? logger = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }



    public DbSet<Employer> Employers { get; set; } = null!;
    public DbSet<MemberEmployment> MemberEmployments { get; set; } = null!;
    public DbSet<CheckoffInstruction> CheckoffInstructions { get; set; } = null!;
    public DbSet<CheckoffBatch> CheckoffBatches { get; set; } = null!;
    public DbSet<CheckoffStagingRow> CheckoffStagingRows { get; set; } = null!;
    public DbSet<CheckoffStagingLoanAllocation> CheckoffStagingLoanAllocations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CheckOffDBContext).Assembly,
            t => t.Namespace?.Contains("Features.CheckOff.Configurations") == true);
    }

    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent>? _collectedDomainEvents;
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = DBContextHelper.CollectDomainEvents(ChangeTracker);
            DBContextHelper.ClearDomainEventsFromAggregates(ChangeTracker);
            DBContextHelper.UpdateAuditAndSoftDelete(ChangeTracker, _actorProvider?.ActorId ?? ICurrentActorProvider.SystemActor, CurrentTenantId);
            var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _collectedDomainEvents ??= [];
            _collectedDomainEvents.AddRange(domainEvents);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
                if (_logger is not null)
                    PersistenceLogDefinitions.LogDatabaseError(_logger, entry.Entity.GetType().Name, ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (DbUpdateException ex)
        {
            foreach (var entry in ex.Entries)
                if (_logger is not null)
                    PersistenceLogDefinitions.LogDatabaseError(_logger, entry.Entity.GetType().Name, ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
        catch (Exception ex)
        {
            if (_logger is not null)
                PersistenceLogDefinitions.LogDBContextSaveChangesError(_logger, nameof(CheckOffDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
