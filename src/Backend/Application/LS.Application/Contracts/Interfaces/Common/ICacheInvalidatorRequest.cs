namespace LS.Application.Contracts.Interfaces.Common;

/// <summary>Declares which tenant-scoped query entries change after a command succeeds.</summary>
public interface ICacheInvalidatorRequest
{
    /// <summary>
    /// Logical entity keys created with CacheKeys.Entity. A new generation invalidates
    /// each matching entity entry, including across instances and user scopes.
    /// </summary>
    IReadOnlyList<string> DirectInvalidationKeys => [];

    /// <summary>
    /// Logical group keys created with CacheKeys.GroupVersion. Without a user ID,
    /// the command invalidates all entries for the group within its tenant.
    /// </summary>
    IReadOnlyList<string> GroupVersionKeysToInvalidate => [];
}
