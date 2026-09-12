using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Shared.Entities;
using LS.Persistence.Common;
using LS.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Persistence.Features.Banking.DataContext;

public class BankingDBContext : DbContext, ITenantFilteredDBContext
{
    private readonly ICurrentTenantProvider? _tenantProvider;
    private readonly ICurrentActorProvider? _actorProvider;
    private readonly ILogger<BankingDBContext>? _logger;

    public BankingDBContext(
        DbContextOptions<BankingDBContext> options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<BankingDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    protected BankingDBContext(
        DbContextOptions options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<BankingDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    public DbSet<ShareProduct> ShareProducts { get; set; }
    public DbSet<ShareAccount> ShareAccounts { get; set; }
    public DbSet<ShareTransaction> ShareTransactions { get; set; }

    public DbSet<SavingsProduct> SavingsProducts { get; set; }
    public DbSet<SavingsAccount> SavingsAccounts { get; set; }
    public DbSet<SavingsTransaction> SavingsTransactions { get; set; }

    public DbSet<DepositProduct> DepositProducts { get; set; }
    public DbSet<DepositAccount> DepositAccounts { get; set; }
    public DbSet<DepositTransaction> DepositTransactions { get; set; }

    public DbSet<LS.Domain.Features.Banking.FOSA.Entities.FosaAccount> FosaAccounts { get; set; }
    public DbSet<LS.Domain.Features.Banking.FOSA.Entities.TellerTill> TellerTills { get; set; }
    public DbSet<LS.Domain.Features.Banking.FOSA.Entities.Vault> Vaults { get; set; }
    public DbSet<LS.Domain.Features.Banking.FOSA.Entities.TillBalancingRecord> TillBalancingRecords { get; set; }
    public DbSet<LS.Domain.Features.Banking.FOSA.Entities.FosaTransaction> FosaTransactions { get; set; }


    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(BankingDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Banking", StringComparison.Ordinal) == true &&
                    !(type.Namespace?.Contains("SqlServer") == true) &&
                    !(type.Namespace?.Contains("PostgreSql") == true));

        DBContextHelper.ApplyStandardModelConventions(modelBuilder, this);
        if (Database.IsNpgsql())
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.RowVersion)).ValueGeneratedNever();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var domainEvents = DBContextHelper.CollectDomainEvents(ChangeTracker);
            DBContextHelper.ClearDomainEventsFromAggregates(ChangeTracker);
            DBContextHelper.UpdateAuditAndSoftDelete(ChangeTracker, _actorProvider?.ActorId ?? ICurrentActorProvider.SystemActor, CurrentTenantId);
            if (Database.IsNpgsql())
                foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified))
                    entry.Entity.RowVersion = Guid.CreateVersion7().ToByteArray();
            var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _collectedDomainEvents ??= [];
            _collectedDomainEvents.AddRange(domainEvents);
            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var entityId = entry.Entity is BaseEntity b ? b.Id.ToString() : "(unknown)";
                if (_logger is not null)
                    PersistenceLogDefinitions.LogConcurrencyConflict(_logger, entry.Entity.GetType().Name, entityId);
                _ = await entry.GetDatabaseValuesAsync(cancellationToken).ConfigureAwait(false);
            }
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
                PersistenceLogDefinitions.LogDBContextSaveChangesError(_logger, nameof(BankingDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
