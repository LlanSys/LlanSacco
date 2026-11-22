using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Behaviours;

public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    ICacheService cache,
    ICurrentTenantProvider tenantProvider,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICacheInvalidatorRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var tenant = tenantProvider.TenantId;
        var keys = tenant == Guid.Empty
            ? Array.Empty<string>()
            : request.GroupVersionKeysToInvalidate.Concat(request.DirectInvalidationKeys)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        var response = await next(cancellationToken).ConfigureAwait(false);

        if (keys.Length == 0 || response is IAppResponse { IsSuccess: false })
        {
            return response;
        }

        // The command may already have committed when the client disconnects.
        // Give invalidation a bounded budget independent of the request's lifetime.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        foreach (var logicalKey in keys)
        {
            var key = CacheKeys.RequestGeneration(tenant, logicalKey);

            try
            {
                var generation = Guid.CreateVersion7().ToString("N");
                await cache.SetGenerationAsync(key, generation, CacheKeys.GenerationLifetime, timeout.Token).ConfigureAwait(false);
                CacheLogDefinitions.LogCacheBumped(logger, key, generation);
            }
            catch (Exception ex)
            {
                // Preserve the committed result. Read expiration bounds staleness
                // if the version write could not reach the shared store.
                CacheLogDefinitions.LogPostCommitInvalidationFailed(logger, typeof(TRequest).Name, key, ex);
            }
        }

        return response;
    }
}
