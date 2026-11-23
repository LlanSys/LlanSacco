using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Behaviours;

public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cache,
    ICurrentTenantProvider tenantProvider,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICachableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var tenant = tenantProvider.TenantId;

        if (request.BypassCache || tenant == Guid.Empty)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        string key;

        try
        {
            var groupKey = CacheKeys.RequestGeneration(tenant, CacheKeys.GroupVersion(request.CacheGroup));
            var groupGeneration = await ResolveGenerationAsync(groupKey, cancellationToken).ConfigureAwait(false);
            var userGeneration = string.IsNullOrWhiteSpace(request.CacheUserId)
                ? null
                : await ResolveGenerationAsync(
                    CacheKeys.RequestGeneration(
                    tenant,
                        CacheKeys.GroupVersion(request.CacheGroup, request.CacheUserId)),
                    cancellationToken)
                    .ConfigureAwait(false);

            var entityGeneration = request.IsVersioned
                ? null
                : await ResolveGenerationAsync(
                    CacheKeys.RequestGeneration(
                    tenant,
                        CacheKeys.Entity(request.CacheGroup, request.Discriminator)),
                    cancellationToken)
                    .ConfigureAwait(false);

            key = CacheKeys.RequestEntry(tenant, request, groupGeneration, userGeneration, entityGeneration);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Without a shared version we cannot know whether a cached value is current.
            CacheLogDefinitions.LogCacheGenerationReadFailed(logger, typeof(TRequest).Name, ex);
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var expiration = request.Expiration ?? TimeSpan.FromMinutes(30);
        return await cache.GetOrCreateAsync(key, next.Invoke, expiration, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> ResolveGenerationAsync(string key, CancellationToken cancellationToken)
    {
        var generation = await cache.GetGenerationAsync(key, cancellationToken).ConfigureAwait(false);

        if (generation is not null)
        {
            return generation;
        }

        // This generation is unique. A concurrent initializer can discard it, but
        // can never revive cached data from an earlier generation.
        generation = Guid.CreateVersion7().ToString("N");
        await cache.SetGenerationAsync(key, generation, CacheKeys.GenerationLifetime, cancellationToken).ConfigureAwait(false);
        return generation;
    }
}
