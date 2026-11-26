using Npgsql;

namespace LS.DatabaseTools;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args is not ["local-status" or "local-upgrade"])
        {
            Console.Error.WriteLine("Usage: LS.DatabaseTools local-status|local-upgrade");
            return 2;
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            var target = LocalDatabaseTarget.Resolve();
            var valid = await LocalMigrationWorkflow.RunAsync(target, apply: args[0] == "local-upgrade", cancellation.Token)
                .ConfigureAwait(false);
            return valid ? 0 : 1;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Local database maintenance was cancelled.");
        }
        catch (LocalDatabaseOperationException exception)
        {
            Console.Error.WriteLine(exception.Message);
        }
        catch (PostgresException exception)
        {
            Console.Error.WriteLine($"Local PostgreSQL operation failed (diagnostic code {exception.SqlState}).");
        }
        catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
        {
            Console.Error.WriteLine("Local migration workflow failed. Check that the local database is running and reachable, and the last context reported above.");
        }

        return 1;
    }
}
