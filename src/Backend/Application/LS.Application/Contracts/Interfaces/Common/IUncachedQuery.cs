namespace LS.Application.Contracts.Interfaces.Common;

/// <summary>Declares a fresh read and records why it bypasses the query cache.</summary>
public interface IUncachedQuery : ICachableRequest
{
    string CacheBypassReason { get; }

    bool ICachableRequest.BypassCache => true;
    string ICachableRequest.CacheGroup => throw new InvalidOperationException("This query bypasses the cache.");
    string ICachableRequest.Discriminator => throw new InvalidOperationException("This query bypasses the cache.");
    string? ICachableRequest.CacheUserId => null;
    bool ICachableRequest.IsVersioned => false;
}
