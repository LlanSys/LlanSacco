using MassTransit.EntityFrameworkCoreIntegration;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Contracts.Implementations.Common;

public sealed class ContextEventPublisher<TUnitOfWork, TContext>(
    IScopedBusOutbox<TContext> outbox, ICurrentTenantProvider tenantProvider, ICurrentActorProvider actorProvider)
    : IContextEventPublisher<TUnitOfWork>
    where TUnitOfWork : ITransactionalUnitOfWork
    where TContext : DbContext
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        return outbox.Publish(message, context =>
        {
            context.Headers.Set("tenant_id", tenantId);
            context.Headers.Set("actor_id", actorProvider.ActorId);
        }, cancellationToken);
    }
}
