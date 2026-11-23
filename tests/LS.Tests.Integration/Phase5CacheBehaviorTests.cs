using FluentValidation;
using LS.Application.Behaviours;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Extensions;
using LS.Application.Utilities;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Caching;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LS.Tests.Integration;

public sealed class Phase5CacheBehaviorTests
{
    [Fact]
    public async Task Pipeline_validates_before_cache_hits_and_separates_tenant_user_filter_and_entity_keys()
    {
        using var store = new SharedDistributedCache();
        var state = new Source();
        await using var first = Provider(store, Guid.CreateVersion7(), state);
        await using var other = Provider(store, Guid.CreateVersion7(), state);
        await using var noTenant = Provider(store, Guid.Empty, state);
        var a = new Read("alice", "page1");
        var b = new Read("bob", "page1");

        using (var scope = first.CreateScope())
        {
            var pipeline = scope.ServiceProvider.GetServices<IPipelineBehavior<Read, AppResponse<string>>>()
                .Select(behavior => behavior.GetType().GetGenericTypeDefinition())
                .ToArray();

            Assert.Equal(
                new[] {
                typeof(ExceptionHandlingBehavior<,>),
                typeof(LoggingBehaviour<,>),
                    typeof(ValidationBehavior<,>),
                typeof(CachingBehavior<,>) },
                pipeline);
        }

        Assert.True((await Send(first, a)).IsSuccess);
        Assert.True((await Send(first, a)).IsSuccess);
        Assert.False((await Send(first, a with { Accepted = false })).IsSuccess);
        Assert.Equal(1, state.ReadCount);

        await Send(first, b);
        await Send(first, a with { Filter = "page2" });
        await Send(first, a with { IsVersioned = false });
        await Send(first, b with { IsVersioned = false });
        await Send(other, a);
        await Send(noTenant, a);
        await Send(noTenant, a);
        Assert.Equal(8, state.ReadCount);
    }

    [Fact]
    public async Task Concurrent_misses_execute_one_factory_and_failed_outcomes_never_reach_either_tier()
    {
        var state = new Source { Value = null, Pause = new Gate() };
        var gate = state.Pause;
        var tenant = Guid.CreateVersion7();
        using var store = new SharedDistributedCache();
        await using var first = Provider(store, tenant, state);
        await using var otherNode = Provider(store, tenant, state);
        var read = new Read("alice", "one");

        var handler = new ReadHandler(state);
        var local = first.GetRequiredService<ICacheService>();
        var remote = otherNode.GetRequiredService<ICacheService>();
        Task<AppResponse<string>> ReadAsync(ICacheService cache)
            => cache.GetOrCreateAsync("coalesced-factory", token => handler.Handle(read, token));

        var start = ReadAsync(local);
        await gate.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var waiters = Enumerable.Range(0, 8).Select(_ => ReadAsync(local)).ToArray();
        gate.Release.TrySetResult();
        Assert.All(await Task.WhenAll(waiters.Append(start)), result => Assert.False(result.IsSuccess));
        Assert.Equal(1, state.ReadCount);

        Assert.Equal(0, store.WriteCount);

        state.Value = "available";
        Assert.Equal("available", (await ReadAsync(remote)).Data);
        Assert.Equal("available", (await ReadAsync(local)).Data);
        Assert.Equal(2, state.ReadCount);
    }

    [Fact]
    public async Task Committed_writes_invalidate_remote_local_entries_and_rejects_leave_them_intact()
    {
        var state = new Source();
        var tenant = Guid.CreateVersion7();
        using var store = new SharedDistributedCache();
        await using var nodeA = Provider(store, tenant, state);
        await using var nodeB = Provider(store, tenant, state);
        var list = new Read("alice", "one");
        var entity = list with { IsVersioned = false };
        var otherUser = list with { User = "bob" };

        Assert.Equal("initial", (await Send(nodeA, list)).Data);
        Assert.Equal("initial", (await Send(nodeA, entity)).Data);
        Assert.Equal("initial", (await Send(nodeA, otherUser)).Data);
        Assert.False((await Send(nodeB, new Write("ignored", Reject: true))).IsSuccess);
        await Send(nodeA, list);
        Assert.Equal(3, state.ReadCount);

        Assert.True((await Send(nodeB, new Write("user update", User: "alice"))).IsSuccess);
        Assert.Equal("user update", (await Send(nodeA, list)).Data);
        Assert.Equal("initial", (await Send(nodeA, otherUser)).Data);

        await Send(nodeB, new Write("entity update", Entity: "one"));
        Assert.Equal("entity update", (await Send(nodeA, entity)).Data);

        await Send(nodeB, new Write("rapid 1"));
        await Send(nodeB, new Write("rapid 2"));
        Assert.Equal("rapid 2", (await Send(nodeA, list)).Data);
        Assert.Equal("rapid 2", (await Send(nodeA, otherUser)).Data);
    }

    [Fact]
    public async Task In_flight_reads_and_expired_generations_do_not_revive_old_values()
    {
        var state = new Source { Pause = new Gate() };
        var gate = state.Pause;
        using var store = new SharedDistributedCache();
        var tenant = Guid.CreateVersion7();
        await using var nodeA = Provider(store, tenant, state);
        await using var nodeB = Provider(store, tenant, state);
        var request = new Read(null, "all");

        var inFlight = Send(nodeA, request);
        await gate.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));

        try
        {
            await Send(nodeB, new Write("committed"));
            Assert.Equal("committed", (await Send(nodeA, request)).Data);
        }
        finally
        {
            gate.Release.TrySetResult();
        }
        Assert.Equal("initial", (await inFlight).Data);
        Assert.Equal("committed", (await Send(nodeA, request)).Data);

        // Simulate shared metadata expiry while old payloads still live in both cache tiers.
        await store.RemoveAsync($"ls:request:v2:{tenant:N}:generation:probe:version:global");
        state.Value = "after expiration";
        Assert.Equal("after expiration", (await Send(nodeA, request)).Data);
    }

    [Fact]
    public async Task Canceled_factory_finishes_before_its_request_releases_the_scope()
    {
        using var store = new SharedDistributedCache();
        await using var provider = Provider(store, Guid.CreateVersion7(), new Source());
        var cache = provider.GetRequiredService<ICacheService>();
        using var ownerCancellation = new CancellationTokenSource();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cleaningUp = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishCleanup = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var owner = cache.GetOrCreateAsync<string>(
            "owner-cancellation",
            async token =>
            {
                started.TrySetResult();

                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return "unreachable";
                }
                finally
                {
                    cleaningUp.TrySetResult();
                    await finishCleanup.Task;
                }
            },
            ct: ownerCancellation.Token);

        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var waiter = cache.GetOrCreateAsync("owner-cancellation", _ => Task.FromResult("live waiter"));
        ownerCancellation.Cancel();

        try
        {
            await cleaningUp.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(owner.IsCompleted);
        }
        finally
        {
            finishCleanup.TrySetResult();
        }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => owner);
        Assert.Equal("live waiter", await waiter.WaitAsync(TimeSpan.FromSeconds(10)));
    }

    private static ServiceProvider Provider(IDistributedCache store, Guid tenant, Source state)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(store);
        services.AddHybridCache();
        services.AddSingleton<ICacheService, HybridCacheService>();
        services.AddScoped<ICurrentTenantProvider>(_ => new Tenant(tenant));
        services.AddSingleton(state);
        services.AddApplicationServices(new ConfigurationBuilder().Build());
        services.AddTransient<IRequestHandler<Read, AppResponse<string>>, ReadHandler>();
        services.AddTransient<IRequestHandler<Write, AppResponse<bool>>, WriteHandler>();
        services.AddTransient<IValidator<Read>>(_ =>
        {
            var validator = new InlineValidator<Read>();
            validator.RuleFor(request => request.Accepted).Equal(true);
            return validator;
        });

        return services.BuildServiceProvider();
    }

    private static async Task<T> Send<T>(ServiceProvider provider, IRequest<T> request)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    private sealed record Read(string? User, string Filter, bool Accepted = true, bool IsVersioned = true)
        : IRequest<AppResponse<string>>, ICachableRequest
    {
        public string CacheGroup => "probe";
        public string Discriminator => Filter;
        public string? CacheUserId => User;
    }

    private sealed record Write(string Value, bool Reject = false, string? User = null, string? Entity = null)
        : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
    {
        public IReadOnlyList<string> DirectInvalidationKeys => Entity is null ? [] : [CacheKeys.Entity("probe", Entity)];
        public IReadOnlyList<string> GroupVersionKeysToInvalidate => Entity is null ? [CacheKeys.GroupVersion("probe", User)] : [];
    }

    private sealed class Tenant(Guid value) : ICurrentTenantProvider
    {
        public Guid TenantId => value;
    }

    private sealed class ReadHandler(Source state) : IRequestHandler<Read, AppResponse<string>>
    {
        public async Task<AppResponse<string>> Handle(Read request, CancellationToken cancellationToken)
        {
            var value = state.Value;
            Interlocked.Increment(ref state.ReadCount);
            var pause = Interlocked.Exchange(ref state.Pause, null);

            if (pause is not null)
            {
                pause.Started.TrySetResult();
                await pause.Release.Task.WaitAsync(cancellationToken);
            }

            return value is null ? AppResponses.NotFound<string>("No current value.") : AppResponses.Success(value);
        }
    }

    private sealed class WriteHandler(Source state) : IRequestHandler<Write, AppResponse<bool>>
    {
        public Task<AppResponse<bool>> Handle(Write request, CancellationToken cancellationToken)
        {
            if (request.Reject)
            {
                return Task.FromResult(AppResponses.Conflict<bool>("Rejected."));
            }
            state.Value = request.Value;
            return Task.FromResult(AppResponses.Success(true));
        }
    }

    // Each HybridCache gets its own local tier. This shared store models the
    // separate distributed service that both nodes can read and write.
    private sealed class SharedDistributedCache : IDistributedCache, IDisposable
    {
        private readonly IDistributedCache _store = CreateStore();
        private int _writeCount;

        public int WriteCount => Volatile.Read(ref _writeCount);

        public void Dispose()
        {
            if (_store is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private static IDistributedCache CreateStore()
        {
            var connection = Environment.GetEnvironmentVariable("LS_CACHE_TEST_REDIS");

            if (string.IsNullOrWhiteSpace(connection))
            {
                return new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            }

            return new RedisCache(Options.Create(new RedisCacheOptions
            {
                Configuration = connection,
                InstanceName = $"ls:phase5-test:{Guid.CreateVersion7():N}:"
            }));
        }

        public byte[]? Get(string key) => _store.Get(key);
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => _store.GetAsync(key, token);
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Interlocked.Increment(ref _writeCount);
            _store.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Interlocked.Increment(ref _writeCount);
            return _store.SetAsync(key, value, options, token);
        }

        public void Refresh(string key) => _store.Refresh(key);
        public Task RefreshAsync(string key, CancellationToken token = default) => _store.RefreshAsync(key, token);
        public void Remove(string key) => _store.Remove(key);
        public Task RemoveAsync(string key, CancellationToken token = default) => _store.RemoveAsync(key, token);
    }

    private sealed class Source
    {
        public string? Value { get; set; } = "initial";
        public int ReadCount;
        public Gate? Pause;
    }

    private sealed class Gate
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
