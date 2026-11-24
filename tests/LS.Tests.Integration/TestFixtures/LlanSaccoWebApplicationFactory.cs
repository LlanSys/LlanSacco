using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LS.Persistence.Features.Accounting.DataContext;
using LS.Persistence.Features.ControlPlane.DataContext;
using LS.Persistence.Features.HR.DataContext;
using LS.Persistence.Features.IAM.DataContext;
using LS.Persistence.Features.Shared.DataContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace LS.Tests.Integration.TestFixtures;

/// <summary>
/// Integration test host that spins up a fresh PostgreSQL Testcontainer, rewires
/// ALL bounded-context DbContexts to point at it, applies migrations, then runs tests.
///
/// Lifecycle (xUnit IClassFixture + IAsyncLifetime):
///   1. Constructor    — builds the Testcontainer object (no Docker interaction yet).
///   2. InitializeAsync — starts the container, sets connection string, then forces
///                        the host to build by accessing Services, and applies migrations.
///   3. Test methods   — call CreateClient(); host is already built at this point.
///   4. DisposeAsync   — stops and removes the container.
/// </summary>
public class LlanSaccoWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private string _containerConnectionString = string.Empty;

    public LlanSaccoWebApplicationFactory()
    {
        // ── Force the environment to Development BEFORE WebApplication.CreateBuilder runs ──
        // Program.cs uses WebApplication.CreateBuilder(args), which reads environment variables
        // *before* ConfigureWebHost gets called. If the CI runs with ASPNETCORE_ENVIRONMENT=Production,
        // Program.cs will attempt to wire up Azure Key Vault and crash with an empty URI.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

        // Force test configuration early so builder.Configuration reads it in Program.cs
        Environment.SetEnvironmentVariable("CacheSettings__Provider", "Memory");
        Environment.SetEnvironmentVariable("DevelopmentSeed__Enabled", "false");

        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("llansacco_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // ── Suppress the development identity seeder and remove external deps ────
        // Program.cs calls SeedDevelopmentIdentityAsync() during host startup (before
        // our schema-creation step can run). If the seeder fires against an empty DB it
        // throws, the host fails to start, and CreateClient() gets
        // "server has not been started". Disabling it here is safe — integration tests
        // set up their own seed data inside each test method.
        //
        // NOTE: We do NOT call UseEnvironment("Testing") here because the application's
        // Program.cs has a !IsDevelopment() guard that enables Azure Key Vault, which
        // would immediately throw "KeyVault:Uri is not configured." We MUST explicitly
        // set the environment to "Development" here to guarantee that guard is bypassed,
        // even if the CI pipeline sets ASPNETCORE_ENVIRONMENT=Production.
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // ── 1. Auth: replace real JWT/Cookie stack with test-header-based handler ──
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    options => { });

            // ── 2. Rewire ALL bounded-context DbContexts to the Testcontainer ──────────
            //
            // Standard WebApplicationFactory pattern:
            //   a) Remove the existing DbContextOptions<T> descriptors registered by Program.cs
            //   b) Re-register them directly with our container connection string
            //
            RewireDbContext<ControlPlaneDBContext, ControlPlanePostgreSqlDBContext>(services, _containerConnectionString);
            RewireDbContext<IamDBContext, IamPostgreSqlDBContext>(services, _containerConnectionString);
            RewireDbContext<SharedDBContext, SharedPostgreSqlDBContext>(services, _containerConnectionString);
            RewireDbContext<HrDBContext, HrPostgreSqlDBContext>(services, _containerConnectionString);
            RewireDbContext<AccountingDBContext, AccountingPostgreSqlDBContext>(services, _containerConnectionString);
        });
    }

    private static void RewireDbContext<TContext, TProviderContext>(
        IServiceCollection services,
        string connectionString)
        where TContext : DbContext
        where TProviderContext : TContext
    {
        // Remove existing DbContextOptions descriptors registered by Program.cs
        var descriptorsToRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>)
             || d.ServiceType == typeof(DbContextOptions<TProviderContext>))
            .ToList();

        foreach (var d in descriptorsToRemove)
        {
            services.Remove(d);
        }

        // Remove existing concrete context registrations
        var concreteToRemove = services
            .Where(d =>
                (d.ServiceType == typeof(TContext) && d.ImplementationType == typeof(TProviderContext))
             || d.ServiceType == typeof(TProviderContext))
            .ToList();

        foreach (var d in concreteToRemove)
        {
            services.Remove(d);
        }

        // Re-register pointing at the Testcontainer. No migration settings needed
        // because we use EnsureCreated / CreateTablesAsync for test schema setup.
        services.AddDbContext<TContext, TProviderContext>((_, options) =>
            options.UseNpgsql(connectionString, npgsql => npgsql.CommandTimeout(60)));
    }

    public async Task InitializeAsync()
    {
        // Step 1: Start the container — must happen before anything else.
        await _dbContainer.StartAsync();
        _containerConnectionString = _dbContainer.GetConnectionString();

        // Step 2: Force the WebApplicationFactory to build the host now (by accessing Services).
        // _containerConnectionString is populated above so ConfigureTestServices wires correctly.
        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;

        // Step 3: Create schema for all bounded contexts.
        //
        // Strategy: EnsureCreatedAsync() for the FIRST context creates the database + its tables.
        // For subsequent contexts we call IRelationalDatabaseCreator.CreateTablesAsync() directly,
        // because EnsureCreated() is NOT additive — once the database has ANY tables, it returns
        // false and skips CreateTables() for every subsequent call.
        //
        // We catch PostgreSQL 42P07 (relation already exists) and 42710 (object already exists)
        // to handle tables/indexes that appear in multiple context models.
        //
        // Order matters: ControlPlane first — TenantConnectionInterceptor queries [Tenants] on
        // every connection open before any other table can be touched.
        await sp.GetRequiredService<ControlPlaneDBContext>().Database.EnsureCreatedAsync();
        await CreateTablesAsync(sp.GetRequiredService<IamDBContext>());
        await CreateTablesAsync(sp.GetRequiredService<SharedDBContext>());
        await CreateTablesAsync(sp.GetRequiredService<HrDBContext>());
        await CreateTablesAsync(sp.GetRequiredService<AccountingDBContext>());
    }

    new public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Creates the tables defined in <paramref name="context"/>'s model against the
    /// already-existing test database, silently ignoring PostgreSQL errors when a table
    /// or index already exists (created by a previously-called context).
    /// </summary>
    private static async Task CreateTablesAsync(DbContext context)
    {
        try
        {
            var creator = context.GetService<IRelationalDatabaseCreator>();
            await creator.CreateTablesAsync();
        }
        catch (Npgsql.PostgresException ex)
            when (ex.SqlState is Npgsql.PostgresErrorCodes.DuplicateTable or Npgsql.PostgresErrorCodes.DuplicateObject)
        {
            // Expected when contexts share a database and tables/indexes overlap.
        }
    }
}
