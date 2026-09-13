using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Entities;
using LS.Persistence.Common;
using LS.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.Accounting.DataContext;

public class AccountingDBContext : DbContext, ITenantFilteredDBContext
{
    private readonly ICurrentTenantProvider? _tenantProvider;
    private readonly ICurrentActorProvider? _actorProvider;
    private readonly ILogger<AccountingDBContext>? _logger;

    public AccountingDBContext(
        DbContextOptions<AccountingDBContext> options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<AccountingDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    protected AccountingDBContext(
        DbContextOptions options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<AccountingDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Journal> Journals { get; set; }
    public DbSet<JournalLine> JournalLines { get; set; }
    public DbSet<TransactionTypeGlMapping> TransactionTypeGlMappings { get; set; }
    public DbSet<AccountingIntegrationError> AccountingIntegrationErrors { get; set; }

    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AccountingDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.Accounting", StringComparison.Ordinal) == true &&
                    !(type.Namespace?.Contains("SqlServer") == true) &&
                    !(type.Namespace?.Contains("PostgreSql") == true));

        DBContextHelper.ApplyStandardModelConventions(modelBuilder, this);
    }

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
                PersistenceLogDefinitions.LogDBContextSaveChangesError(_logger, nameof(AccountingDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
