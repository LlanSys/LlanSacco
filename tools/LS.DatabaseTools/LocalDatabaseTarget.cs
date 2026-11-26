using System.Net;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LS.DatabaseTools;

/// <summary>
/// The single local PostgreSQL database every bounded context migrates. Remote servers and system databases are
/// refused, so this workflow can never touch a shared or cloud environment.
/// </summary>
internal sealed class LocalDatabaseTarget
{
    internal const string ConnectionVariable = "LS_DATABASE_CONNECTION";
    private const string ApiUserSecretsId = "a2c535b2-27f0-4cc1-92ab-35254c55da19";

    // Every connection name a design-time factory may read. They must all point at the same database.
    internal static readonly string[] ConnectionNames =
    [
        "DefaultConnection", "DefaultPostgreSqlConnection", "AccountingConnection", "BankingConnection",
        "ControlPlaneConnection", "HrConnection", "IamConnection", "MembershipConnection", "SharedConnection",
    ];

    public LocalDatabaseTarget(string connection)
    {
        var settings = new NpgsqlConnectionStringBuilder(connection);
        if (!IsLoopback(settings.Host))
        {
            throw new LocalDatabaseOperationException("Choose a single local database server. Remote database targets are refused.");
        }

        if (string.IsNullOrWhiteSpace(settings.Database)
            || settings.Database is "postgres" or "template0" or "template1")
        {
            throw new LocalDatabaseOperationException("Choose an explicit local application database.");
        }

        Connection = connection;
    }

    public string Connection { get; }

    public static LocalDatabaseTarget Resolve()
    {
        var explicitConnection = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (!string.IsNullOrWhiteSpace(explicitConnection))
        {
            return new LocalDatabaseTarget(explicitConnection);
        }

        var apiDirectory = Path.GetFullPath("src/Backend/Api/LS.Api");
        if (!File.Exists(Path.Combine(apiDirectory, "LS.Api.csproj")))
        {
            throw new LocalDatabaseOperationException("Run the local database command from the LlanSacco repository root.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(ApiUserSecretsId)
            .AddEnvironmentVariables()
            .Build();

        var connection = configuration.GetConnectionString("DefaultPostgreSqlConnection");
        if (string.IsNullOrWhiteSpace(connection))
        {
            connection = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connection) || connection.StartsWith("SET_VIA_", StringComparison.OrdinalIgnoreCase)
            || connection.StartsWith("REPLACE_VIA_", StringComparison.OrdinalIgnoreCase))
        {
            throw new LocalDatabaseOperationException(
                $"Set the API's ConnectionStrings:DefaultConnection in user secrets, set {ConnectionVariable}, or use -LocalCompose.");
        }

        foreach (var name in ConnectionNames)
        {
            var scoped = configuration.GetConnectionString(name);
            if (!string.IsNullOrWhiteSpace(scoped) && !string.Equals(scoped, connection, StringComparison.Ordinal))
            {
                throw new LocalDatabaseOperationException($"The API overrides {name}. This local workflow requires matching context connections.");
            }
        }

        return new LocalDatabaseTarget(connection);
    }

    internal static bool IsLoopback(string? host)
    {
        host = host?.Trim('[', ']');
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
    }
}
