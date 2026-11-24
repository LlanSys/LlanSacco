using MassTransit.DependencyInjection;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.Middleware.Outbox;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Contracts.Implementations.Common;

/// <summary>
/// Stages an integration event in the owning bounded context's EF outbox so it commits with that
/// context's unit of work. MassTransit 8.x configures one default scoped bus outbox, so each
/// context binds its own <see cref="EntityFrameworkScopedBusContext{TBus,TDbContext}"/> explicitly,
/// following the StarterTemplate pattern.
/// </summary>
public sealed class ContextEventPublisher<TUnitOfWork, TContext>(
    IBus bus, TContext context, IBusOutboxNotification notification,
    Bind<IBus, IClientFactory> clientFactory, IServiceProvider services,
    ICurrentTenantProvider tenantProvider, ICurrentActorProvider actorProvider)
    : IContextEventPublisher<TUnitOfWork>
    where TUnitOfWork : ITransactionalUnitOfWork
    where TContext : DbContext
{
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        var tenantId = tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
        {
            throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        }

        // A fresh scoped context per event keeps each staged message independent of an earlier
        // transaction attempt in this DI scope. A UoW retry clears EF tracking after rollback, so
        // nothing from a failed attempt (business writes or messages) is carried forward.
        using var outbox = new EntityFrameworkScopedBusContext<IBus, TContext>(
            bus, context, notification, clientFactory.Value, services);

        await outbox.PublishEndpoint.Publish(
            message,
            sendContext =>
            {
                sendContext.Headers.Set("tenant_id", tenantId);
                sendContext.Headers.Set("actor_id", actorProvider.ActorId);
            },
            cancellationToken)
            .ConfigureAwait(false);
    }
}
