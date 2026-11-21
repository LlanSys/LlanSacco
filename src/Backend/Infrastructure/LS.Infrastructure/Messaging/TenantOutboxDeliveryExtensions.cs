using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Messaging;

public static class TenantOutboxDeliveryExtensions
{
    public static void RouteEntityFrameworkOutboxDeliveryByTenant(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<TenantOutboxRouting>();
        services.AddHostedService(sp => sp.GetRequiredService<TenantOutboxRouting>());

        for (var i = 0; i < services.Count; i++)
        {
            var descriptor = services[i];
            if (descriptor.ServiceType != typeof(IAgentRegistration<IBus>))
                continue;

            services[i] = ServiceDescriptor.Describe(descriptor.ServiceType, sp =>
            {
                var registration = (IAgentRegistration<IBus>)(descriptor.ImplementationInstance
                    ?? descriptor.ImplementationFactory?.Invoke(sp)
                    ?? ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!));
                return registration is IEntityFrameworkOutboxDeliveryRegistration
                    ? new RoutedRegistration(registration, sp.GetRequiredService<TenantOutboxRouting>())
                    : registration;
            }, descriptor.Lifetime);
        }
    }

    private sealed class RoutedRegistration(IAgentRegistration<IBus> inner, TenantOutboxRouting routing)
        : IAgentRegistration<IBus>
    {
        public IAgent CreateAgent(IServiceProvider provider, IBus bus)
            => inner.CreateAgent(new RoutedProvider(provider, routing), bus);
    }

    private sealed class RoutedProvider(IServiceProvider inner, TenantOutboxRouting routing)
        : IServiceProvider, IServiceScopeFactory
    {
        private int _cursor = -1;
        public object? GetService(Type serviceType)
            // ActivatorUtilities injects IServiceProvider into the delivery agent.
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
