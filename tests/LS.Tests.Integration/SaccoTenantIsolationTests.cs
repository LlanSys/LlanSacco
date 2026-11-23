using LS.Application.Features.Membership.Commands;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Features.CheckOff.Entities;
using LS.Domain.Features.Dividends.Entities;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;
using LS.Persistence.Features.Accounting.DataContext;
using LS.Persistence.Features.Banking.DataContext;
using LS.Persistence.Features.CheckOff.DataContext;
using LS.Persistence.Features.Dividends.DataContext;
using LS.Persistence.Features.Loans.DataContext;
using LS.Persistence.Features.Membership.DataContext;
using LS.Persistence.Features.Membership.Extensions;
using LS.Tests.Integration.TestFixtures;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Npgsql;

namespace LS.Tests.Integration;

public abstract class SaccoTenantIsolationTests<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : DbFixture
{
    private const string ActorId = "00000000-0000-0000-0000-000000000001";

    [Fact]
    [Trait("TenantIsolation", nameof(AccountingDBContext))]
    public Task Accounting_QueriesAreScopedToTheCurrentTenant() =>
        AssertTenantIsolationAsync<AccountingDBContext, Account>(
            static (options, tenant, actor) => new AccountingDBContext(options, tenant, actor),
            static (tenant, index) => Account.Create(
                tenant, $"TEST-{index}", "Test account", AccountType.Asset, null, ActorId));

    [Fact]
    [Trait("TenantIsolation", nameof(BankingDBContext))]
    public Task Banking_QueriesAreScopedToTheCurrentTenant() =>
        AssertTenantIsolationAsync<BankingDBContext, SavingsProduct>(
            static (options, tenant, actor) => new BankingDBContext(options, tenant, actor),
            static (_, index) => SavingsProduct.Create(
                "Test savings", $"TEST-{index}", 0, 0, true, 0, null, null, true, ActorId));

    [Fact]
    [Trait("TenantIsolation", nameof(CheckOffDBContext))]
    public Task CheckOff_QueriesAreScopedToTheCurrentTenant() =>
        AssertTenantIsolationAsync<CheckOffDBContext, Employer>(
            static (options, tenant, actor) => new CheckOffDBContext(options, tenant, actor),
            static (tenant, index) => Employer.Create(
                tenant, $"Employer {index}", "Contact", $"sacco-{index}@example.test", $"+25470000000{index}", ActorId));

    [Fact]
    [Trait("TenantIsolation", nameof(DividendsDBContext))]
    public Task Dividends_QueriesAreScopedToTheCurrentTenant() =>
        fixture is PostgreSqlDbFixture
            ? AssertTenantIsolationAsync<DividendsPostgreSqlDBContext, DividendDeclaration>(
                static (options, tenant, actor) => new DividendsPostgreSqlDBContext(options, tenant, actor),
                CreateDividendDeclaration,
                useMigrations: true)
            : AssertTenantIsolationAsync<DividendsSqlServerDBContext, DividendDeclaration>(
                static (options, tenant, actor) => new DividendsSqlServerDBContext(options, tenant, actor),
                CreateDividendDeclaration,
                useMigrations: true);

    [Fact]
    [Trait("TenantIsolation", nameof(LoansDBContext))]
    public Task Loans_QueriesAreScopedToTheCurrentTenant() =>
        AssertTenantIsolationAsync<LoansDBContext, LoanProduct>(
            static (options, tenant, actor) => new LoansDBContext(options, tenant, actor),
            static (tenant, index) => LoanProduct.Create(
                tenant, $"TEST-{index}", "Test loan", null, 0, InterestMethod.Flat, 12, 10000, ActorId));

    [Fact]
    [Trait("TenantIsolation", nameof(MembershipDBContext))]
    public Task Membership_QueriesAreScopedToTheCurrentTenant() =>
        AssertTenantIsolationAsync<MembershipDBContext, Member>(
            static (options, tenant, actor) => new MembershipDBContext(options, tenant, actor),
            CreateMember);

    [Fact]
    public async Task Membership_RemovingBeneficiaryKeepsTheAuditedRowInTheDatabase()
    {
        await using var database = new MembershipDBContext(CreateOptions<MembershipDBContext>());
        var tenant = new Tenant(Guid.CreateVersion7());
        var actor = new Actor();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:MembershipConnection"] = database.Database.GetConnectionString(),
            ["DatabaseSettings:Provider"] = fixture is PostgreSqlDbFixture ? "PostgreSql" : "SqlServer"
        })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICurrentTenantProvider>(tenant);
        services.AddSingleton<ICurrentActorProvider>(actor);
        services.AddSingleton(Substitute.For<ITenantConnectionResolver>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RemoveBeneficiaryCommand>());
        services.AddMembershipPersistence(configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MembershipDBContext>();
        await context.Database.MigrateAsync();

        var member = CreateMember(tenant.TenantId, 1);
        var beneficiary = Beneficiary.Create(
            tenant.TenantId, member.Id, "Test Beneficiary", "Sibling", "+254700000009", "ID9", 100, false, ActorId);

        context.AddRange(member, beneficiary);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(new RemoveBeneficiaryCommand(member.Id, beneficiary.Id));

        Assert.True(result.IsSuccess, result.Error?.Message);
        context.ChangeTracker.Clear();
        Assert.False(await context.Beneficiaries.AnyAsync(row => row.Id == beneficiary.Id));

        var retained = await context.Beneficiaries.IgnoreQueryFilters()
            .SingleAsync(row => row.TenantId == tenant.TenantId && row.Id == beneficiary.Id);

        Assert.True(retained.IsDeleted);
        Assert.Equal(ActorId, retained.DeletedBy);
        Assert.NotNull(retained.DeletedAt);
    }

    private static DividendDeclaration CreateDividendDeclaration(Guid tenant, int _) =>
        DividendDeclaration.Create(tenant, 2026, 0.1m, 0.05m, createdBy: ActorId);

    private static Member CreateMember(Guid tenant, int index) =>
        Member.Create(
            tenant,
            $"TEST-{index}",
            "Test",
            "Member",
            $"member-{index}@example.test",
            $"+25470000000{index}",
            $"ID{index}",
            new DateOnly(1990, 1, 1),
            default,
            ActorId);

    private async Task AssertTenantIsolationAsync<TContext, TEntity>(
        Func<DbContextOptions<TContext>, ICurrentTenantProvider, ICurrentActorProvider, TContext> createContext,
        Func<Guid, int, TEntity> createEntity,
        bool useMigrations = false)
        where TContext : DbContext
        where TEntity : BaseEntity
    {
        var options = CreateOptions<TContext>();
        var firstTenant = new Tenant(Guid.CreateVersion7());
        var secondTenant = new Tenant(Guid.CreateVersion7());
        var actor = new Actor();
        var firstRow = createEntity(firstTenant.TenantId, 1);
        var secondRow = createEntity(secondTenant.TenantId, 2);

        await using (var firstWriter = createContext(options, firstTenant, actor))
        {
            if (useMigrations)
            {
                await firstWriter.Database.MigrateAsync();
            }
            else
            {
                await firstWriter.Database.EnsureCreatedAsync();
            }

            firstWriter.Set<TEntity>().Add(firstRow);
            await firstWriter.SaveChangesAsync();
        }

        await using (var secondWriter = createContext(options, secondTenant, actor))
        {
            secondWriter.Set<TEntity>().Add(secondRow);
            await secondWriter.SaveChangesAsync();
        }

        await using var firstReader = createContext(options, firstTenant, actor);
        await using var secondReader = createContext(options, secondTenant, actor);
        await using var unscopedReader = createContext(options, new Tenant(Guid.Empty), actor);

        var firstVisible = Assert.Single(await firstReader.Set<TEntity>().AsNoTracking().ToListAsync());
        var secondVisible = Assert.Single(await secondReader.Set<TEntity>().AsNoTracking().ToListAsync());

        Assert.Equal(firstRow.Id, firstVisible.Id);
        Assert.Equal(firstTenant.TenantId, firstVisible.TenantId);
        Assert.Equal(secondRow.Id, secondVisible.Id);
        Assert.Equal(secondTenant.TenantId, secondVisible.TenantId);
        Assert.Equal(ActorId, firstVisible.CreatedBy);
        Assert.Equal(ActorId, secondVisible.CreatedBy);
        Assert.False(await firstReader.Set<TEntity>().AnyAsync(row => row.Id == secondRow.Id));
        Assert.False(await secondReader.Set<TEntity>().AnyAsync(row => row.Id == firstRow.Id));
        Assert.False(await unscopedReader.Set<TEntity>().AnyAsync());
    }

    private DbContextOptions<TContext> CreateOptions<TContext>()
        where TContext : DbContext
    {
        var databaseName = $"tenant_{Guid.CreateVersion7():N}";
        var options = new DbContextOptionsBuilder<TContext>();

        if (fixture is PostgreSqlDbFixture)
        {
            var connection = new NpgsqlConnectionStringBuilder(fixture.GetConnectionString()) { Database = databaseName };
            options.UseNpgsql(connection.ConnectionString);
        }
        else
        {
            var connection = new SqlConnectionStringBuilder(fixture.GetConnectionString()) { InitialCatalog = databaseName };
            options.UseSqlServer(connection.ConnectionString);
        }

        return options.Options;
    }

    private sealed class Tenant(Guid tenantId) : ICurrentTenantProvider
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class Actor : ICurrentActorProvider
    {
        string ICurrentActorProvider.ActorId => ActorId;
    }
}
