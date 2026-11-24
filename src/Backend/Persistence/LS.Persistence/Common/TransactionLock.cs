using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Common;

internal static class TransactionLock
{
    internal static async Task AcquireAsync(DbContext context, string resource, CancellationToken ct)
    {
        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A transaction lock requires an owning database transaction.");
        }

        if (context.Database.IsSqlServer())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @result < 0 THROW 51000, 'Transaction lock unavailable.', 1;", ct).ConfigureAwait(false);
        }
        else if (context.Database.IsNpgsql())
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(resource));
            var key = BinaryPrimitives.ReadInt64LittleEndian(hash);
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key});", ct).ConfigureAwait(false);
        }
        else
        {
            throw new NotSupportedException("Transaction locks require PostgreSQL or SQL Server.");
        }
    }
}
