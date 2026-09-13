using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Domain.Features.HR.Departments.Entities;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.Domain.Shared.Entities;
using LS.Persistence.Common;
using LS.Persistence.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.HR.DataContext;

public class HrDBContext : DbContext, ITenantFilteredDBContext
{
    private readonly ICurrentTenantProvider? _tenantProvider;
    private readonly ICurrentActorProvider? _actorProvider;
    private readonly ILogger<HrDBContext>? _logger;

    public HrDBContext(
        DbContextOptions<HrDBContext> options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<HrDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }

    protected HrDBContext(
        DbContextOptions options,
        ICurrentTenantProvider? tenantProvider = null,
        ICurrentActorProvider? actorProvider = null,
        ILogger<HrDBContext>? logger = null
    ) : base(options)
    {
        _tenantProvider = tenantProvider;
        _actorProvider = actorProvider;
        _logger = logger;
    }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeNumberSequence> EmployeeNumberSequences { get; set; }
    public DbSet<Department> Departments { get; set; }

    // Payroll DbSets
    public DbSet<PayrollStatutoryConfiguration> PayrollStatutoryConfigurations { get; set; }
    public DbSet<PayeTaxBand> PayeTaxBands { get; set; }
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
    public DbSet<PayrollComponent> PayrollComponents { get; set; }
    public DbSet<EmployeePayrollComponent> EmployeePayrollComponents { get; set; }
    public DbSet<Payslip> Payslips { get; set; }
    public DbSet<PayslipDetail> PayslipDetails { get; set; }

    public Guid CurrentTenantId => _tenantProvider?.TenantId ?? Guid.Empty;

    private List<IDomainEvent> _collectedDomainEvents = [];
    public IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents() => _collectedDomainEvents?.AsReadOnly();
    public void ClearCollectedDomainEvents() => _collectedDomainEvents?.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HrDBContext).Assembly,
            type => type.Namespace?.StartsWith("LS.Persistence.Features.HR", StringComparison.Ordinal) == true &&
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
                PersistenceLogDefinitions.LogDBContextSaveChangesError(_logger, nameof(HrDBContext), ex);
            _collectedDomainEvents?.Clear();
            throw;
        }
    }
}
