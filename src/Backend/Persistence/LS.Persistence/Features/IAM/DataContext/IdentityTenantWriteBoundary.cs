using LS.Domain.Features.IAM.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LS.Persistence.Features.IAM.DataContext;

/// <summary>
/// Identity join rows (user roles, user claims, logins, tokens, sessions, devices) carry no tenant column, so the
/// tenant query filter cannot protect them. Any write that references a user must reference a user of the current
/// tenant. Ported from StarterTemplate; roles are platform-wide in LlanSacco, so only users are checked.
/// </summary>
internal static class IdentityTenantWriteBoundary
{
    public static async Task EnforceAsync(IamDBContext context, Guid tenantId, CancellationToken cancellationToken)
    {
        var userIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in context.ChangeTracker.Entries().Where(entry =>
            entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            foreach (var foreignKey in entry.Metadata.GetForeignKeys())
            {
                if (foreignKey.PrincipalEntityType.ClrType == typeof(AppUser))
                {
                    Collect(entry, foreignKey.Properties[0].Name, userIds);
                }
            }
        }

        if (userIds.Count == 0)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<AppUser>()
            .Where(entry => entry.State == EntityState.Added && entry.Entity.TenantId == tenantId))
        {
            userIds.Remove(entry.Entity.Id);
        }

        if (userIds.Count == 0)
        {
            return;
        }

        var found = await context.Users.IgnoreQueryFilters()
            .CountAsync(user => userIds.Contains(user.Id) && user.TenantId == tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (found != userIds.Count)
        {
            throw new InvalidOperationException("An identity write must target a user in the current workspace.");
        }
    }

    private static void Collect(EntityEntry entry, string propertyName, HashSet<string> ids)
    {
        var property = entry.Property(propertyName);
        if (property.CurrentValue is string current)
        {
            ids.Add(current);
        }

        if (entry.State != EntityState.Added && property.OriginalValue is string original)
        {
            ids.Add(original);
        }
    }
}
