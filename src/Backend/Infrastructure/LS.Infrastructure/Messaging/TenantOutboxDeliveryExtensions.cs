using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LS.Infrastructure.Messaging;

public static class TenantOutboxDeliveryExtensions
{
    /// <summary>
    /// Routes each MassTransit 8.x EF outbox delivery worker (one hosted
    /// <c>BusOutboxDeliveryService&lt;TDbContext&gt;</c> per context) through tenant-initialized scopes,
    /// so delivery queries run under a resolved tenant like any other background work.
    /// </summary>
    public static void RouteEntityFrameworkOutboxDeliveryByTenant(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<TenantOutboxRouting>();
        services.AddHostedService(sp => sp.GetRequiredService<TenantOutboxRouting>());

        var routed = 0;
        for (var i = 0; i < services.Count; i++)
        {
            var descriptor = services[i];
            if (descriptor.ServiceType != typeof(IHostedService) || !IsOutboxDeliveryService(descriptor.ImplementationType))
                continue;

            var implementationType = descriptor.ImplementationType!;
            services[i] = ServiceDescriptor.Describe(
                typeof(IHostedService),
                sp => (IHostedService)ActivatorUtilities.CreateInstance(
                    new RoutedProvider(sp, sp.GetRequiredService<TenantOutboxRouting>()), implementationType),
                descriptor.Lifetime);
            routed++;
        }

        if (routed == 0)
        {
            // Fail at startup rather than silently delivering without tenant scopes.
            throw new InvalidOperationException(
                "No MassTransit EF outbox delivery services were registered before tenant routing was applied.");
        }
    }

    private static bool IsOutboxDeliveryService(Type? type)
        => type is { IsGenericType: true }
            && type.GetGenericTypeDefinition().Name.StartsWith("BusOutboxDeliveryService", StringComparison.Ordinal)
            && type.Namespace?.StartsWith("MassTransit", StringComparison.Ordinal) == true;

    private sealed class RoutedProvider(IServiceProvider inner, TenantOutboxRouting routing)
        : IServiceProvider, IServiceScopeFactory
    {
        private int _cursor = -1;
        public object? GetService(Type serviceType)
            // ActivatorUtilities injects IServiceProvider into the delivery service.
            => serviceType == typeof(IServiceScopeFactory) || serviceType == typeof(IServiceProvider)
                ? this
                : inner.GetService(serviceType);

        public IServiceScope CreateScope()
        {
            var scope = inner.CreateScope();
            var tenantId = routing.NextTenant(ref _cursor);
            if (tenantId.HasValue)
                scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>()
                    .Initialize(tenantId.Value, ICurrentActorProvider.SystemActor);
            return scope;
        }
    }
}
