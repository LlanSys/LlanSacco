using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.Shared.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LS.Tests.Architecture.Database;

/// <summary>
/// Discovers every migration context instead of listing them by hand, so a new bounded context cannot
/// ship without a current migration or without a deployment bundle. LlanSacco runs on PostgreSQL only.
/// </summary>
[Trait("Category", "Guardrail")]
public class MigrationCoverageGuardrailTests
{
    private const string PostgreSqlSuffix = "PostgreSqlDBContext";

    public static TheoryData<Type> ProviderContexts()
    {
        var data = new TheoryData<Type>();

        foreach (var type in DiscoverProviderContexts())
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ProviderContexts))]
    public void ProviderContext_ShouldNotHavePendingModelChanges(Type contextType)
    {
        var services = new ServiceCollection();

        typeof(MigrationCoverageGuardrailTests)
            .GetMethod(nameof(AddContext), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(contextType)
            .Invoke(null, [services]);

        services.AddScoped<ICurrentTenantProvider, DummyTenantProvider>();
        services.AddScoped<ICurrentActorProvider, DummyActorProvider>();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);

        Assert.False(
            context.Database.HasPendingModelChanges(),
            $"The model for {contextType.Name} has changed since its last migration. Add a migration for every supported provider.");
    }

    [Fact]
    public void EveryMigrationContext_ShouldTargetPostgreSql()
    {
        var nonPostgreSql = typeof(SharedDBContext).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(DbContext).IsAssignableFrom(type)
                && type.Name.EndsWith("DBContext", StringComparison.Ordinal)
                && type.Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            .Select(type => type.Name)
            .ToList();

        Assert.Empty(nonPostgreSql);
    }

    [Fact]
    public void DeploymentContextList_ShouldMatchEveryMigrationContext()
    {
        var path = Path.Combine(FindRepositoryRoot(), "ops", "migrations", "contexts.txt");
        var listed = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToList();

        Assert.Equal(listed.Count, listed.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(ContextNames(PostgreSqlSuffix), listed.Order(StringComparer.Ordinal).ToList());
    }

    private static void AddContext<TContext>(IServiceCollection services) where TContext : DbContext
        => services.AddDbContext<TContext>(options =>
            options.UseNpgsql("Host=localhost;Database=dummy;Username=postgres;Password=postgres"));

    private static IEnumerable<Type> DiscoverProviderContexts()
        => typeof(SharedDBContext).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && typeof(DbContext).IsAssignableFrom(type)
                && type.Name.EndsWith(PostgreSqlSuffix, StringComparison.Ordinal))
            .OrderBy(type => type.Name, StringComparer.Ordinal);

    private static List<string> ContextNames(string suffix)
        => DiscoverProviderContexts()
            .Where(type => type.Name.EndsWith(suffix, StringComparison.Ordinal))
            .Select(type => type.Name[..^suffix.Length])
            .Order(StringComparer.Ordinal)
            .ToList();

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlanSacco.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }

    private sealed class DummyTenantProvider : ICurrentTenantProvider
    {
        public Guid TenantId => Guid.Empty;
    }

    private sealed class DummyActorProvider : ICurrentActorProvider
    {
        public string ActorId => "System";
    }
}
