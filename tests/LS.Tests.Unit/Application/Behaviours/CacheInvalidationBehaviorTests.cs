using LS.Application.Behaviours;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace LS.Tests.Unit.Application.Behaviours;

public sealed class CacheInvalidationBehaviorTests
{
    private static readonly Guid TenantId = Guid.Parse("0194f700-0000-7000-8000-000000000001");

    [Fact]
    public async Task Bumps_the_tenant_generation_even_if_the_client_disconnects_after_commit()
    {
        var cache = new RecordingCacheService();
        var behavior = Behavior(cache);
        using var requestCancellation = new CancellationTokenSource();

        var result = await behavior.Handle(
            new(),
            _ =>
            {
                requestCancellation.Cancel();
                return Task.FromResult(AppResponses.Success(true));
            },
            requestCancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal($"ls:request:v2:{TenantId:N}:generation:lookups:version:global", Assert.Single(cache.GenerationKeys));
    }

    [Fact]
    public async Task A_rejection_leaves_the_generation_unchanged()
    {
        var cache = new RecordingCacheService();
        var result = await Behavior(cache).Handle(
            new(),
            _ => Task.FromResult(AppResponses.Conflict<bool>("Request rejected.")),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(cache.GenerationKeys);
    }

    [Fact]
    public async Task A_cache_fault_preserves_the_result_and_does_not_repeat_the_write()
    {
        var cache = new RecordingCacheService { FailWrites = true };
        var handlerCalls = 0;

        var result = await Behavior(cache).Handle(
            new(),
            _ =>
            {
                handlerCalls++;
                return Task.FromResult(AppResponses.Success(true));
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, handlerCalls);
        Assert.Single(cache.GenerationKeys);
    }

    private static CacheInvalidationBehavior<TestInvalidationRequest, AppResponse<bool>> Behavior(RecordingCacheService cache)
        => new(cache, new FixedTenantProvider(TenantId), NullLogger<CacheInvalidationBehavior<TestInvalidationRequest, AppResponse<bool>>>.Instance);

    private sealed record TestInvalidationRequest : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
    {
        public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("lookups")];
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ICurrentTenantProvider
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class RecordingCacheService : ICacheService
    {
        public bool FailWrites { get; init; }
        public List<string> GenerationKeys { get; } = [];

        public Task<string?> GetGenerationAsync(string key, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task SetGenerationAsync(string key, string generation, TimeSpan expiration, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GenerationKeys.Add(key);
            return FailWrites ? Task.FromException(new InvalidOperationException("Version unavailable.")) : Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task RemoveAsync(string key, CancellationToken ct = default) => throw new NotSupportedException();
        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default) => throw new NotSupportedException();

        public Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken ct = default) => throw new NotSupportedException();
    }
}
