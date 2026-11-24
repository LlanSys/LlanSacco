using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Common;

/// <summary>
/// Serializes short critical sections with a transaction-scoped PostgreSQL advisory lock. The lock is
/// released when the owning transaction commits or rolls back.
/// </summary>
internal static class TransactionLock
{
    internal static async Task AcquireAsync(DbContext context, string resource, CancellationToken ct)
    {
        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A transaction lock requires an owning database transaction.");
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(resource));
        var key = BinaryPrimitives.ReadInt64LittleEndian(hash);

        // Fail after 10 seconds instead of blocking indefinitely. SET LOCAL scopes the timeout to this
        // transaction, so pooled connections are unaffected.
        await context.Database.ExecuteSqlRawAsync("SET LOCAL lock_timeout = '10s';", ct).ConfigureAwait(false);
        await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key});", ct).ConfigureAwait(false);
    }
}
