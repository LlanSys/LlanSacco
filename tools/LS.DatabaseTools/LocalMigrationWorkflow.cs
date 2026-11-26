using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.DatabaseTools;

/// <summary>
/// Reports and applies pending migrations for every bounded context in ops/migrations/contexts.txt, in that order.
/// It refuses histories that do not belong to this checkout (unknown or skipped migrations) and models that differ
/// from their snapshots, instead of migrating a database into an unknown state.
/// </summary>
internal static class LocalMigrationWorkflow
{
    public static async Task<bool> RunAsync(LocalDatabaseTarget target, bool apply, CancellationToken cancellationToken)
    {
        // The design-time factories read their connection from configuration; point every name at the target.
        foreach (var name in LocalDatabaseTarget.ConnectionNames)
        {
            Environment.SetEnvironmentVariable($"ConnectionStrings__{name}", target.Connection);
        }

        var contexts = CreateContexts();
        try
        {
            var connection = contexts[0].Database.GetDbConnection();
            Console.WriteLine($"Local database '{connection.Database}' on '{connection.DataSource}'.");

            if (!await contexts[0].GetService<IRelationalDatabaseCreator>().ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                throw new LocalDatabaseOperationException("The local database is missing. Use the documented fresh database setup.");
            }

            var pendingContexts = await ReportAsync(contexts, cancellationToken).ConfigureAwait(false);
            if (pendingContexts is null)
            {
                return false;
            }

            if (!apply || pendingContexts.Count == 0)
            {
                return true;
            }

            foreach (var context in pendingContexts)
            {
                Console.WriteLine($"Applying migrations for {context.GetType().Name}.");
                await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }

            var remaining = await ReportAsync(contexts, cancellationToken).ConfigureAwait(false);
            return remaining is { Count: 0 };
        }
        finally
        {
            foreach (var context in contexts)
            {
                await context.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static List<DbContext> CreateContexts()
    {
        var manifest = Path.GetFullPath("ops/migrations/contexts.txt");
        if (!File.Exists(manifest))
        {
            throw new LocalDatabaseOperationException("Run the local database command from the LlanSacco repository root.");
        }

        var persistence = typeof(IamPostgreSqlDBContextFactory).Assembly;
        var contexts = new List<DbContext>();
        foreach (var name in File.ReadLines(manifest).Select(static line => line.Trim()).Where(static line => line.Length > 0 && !line.StartsWith('#')))
        {
            var factoryType = persistence.GetTypes().SingleOrDefault(type =>
                type.Name == $"{name}PostgreSqlDBContextFactory" && !type.IsAbstract);
            if (factoryType is null)
            {
                throw new LocalDatabaseOperationException($"No design-time factory '{name}PostgreSqlDBContextFactory' exists for context '{name}'.");
            }

            var create = factoryType.GetInterfaces()
                .Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>))
                .GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext))!;
            contexts.Add((DbContext)create.Invoke(Activator.CreateInstance(factoryType), [Array.Empty<string>()])!);
        }

        return contexts;
    }

    private static async Task<List<DbContext>?> ReportAsync(List<DbContext> contexts, CancellationToken cancellationToken)
    {
        var valid = true;
        var pendingContexts = new List<DbContext>();
        foreach (var context in contexts)
        {
            var pending = await CheckContextAsync(context, cancellationToken).ConfigureAwait(false);
            if (pending is null)
            {
                valid = false;
            }
            else if (pending.Length > 0)
            {
                pendingContexts.Add(context);
            }
        }

        return valid ? pendingContexts : null;
    }

    private static async Task<string[]?> CheckContextAsync(DbContext context, CancellationToken cancellationToken)
    {
        var name = context.GetType().Name;
        var known = context.Database.GetMigrations().ToArray();
        var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false))
            .ToHashSet(StringComparer.Ordinal);

        if (known.Length > 0 && applied.Count > 0 && !applied.Contains(known[0]))
        {
            Console.Error.WriteLine($"{name}: this checkout's initial migration is missing from the local history. Verify the target and use the documented fresh setup for a new database.");
            return null;
        }

        var unknown = applied.Except(known, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if (unknown.Length > 0)
        {
            Console.Error.WriteLine($"{name}: migrations absent from this checkout: {string.Join(", ", unknown)}.");
            return null;
        }

        var firstGap = known.Take(applied.Count).FirstOrDefault(migration => !applied.Contains(migration));
        if (firstGap is not null)
        {
            Console.Error.WriteLine($"{name}: the history skips '{firstGap}' before a later applied migration.");
            return null;
        }

        if (context.GetService<IMigrationsAssembly>().ModelSnapshot is null || context.Database.HasPendingModelChanges())
        {
            Console.Error.WriteLine($"{name}: the model differs from its migration snapshot. Scaffold and review a migration.");
            return null;
        }

        var pending = known.Skip(applied.Count).ToArray();
        Console.WriteLine($"{name}: {pending.Length} pending migrations.");
        foreach (var migration in pending)
        {
            Console.WriteLine($"  {migration}");
        }

        return pending;
    }
}
