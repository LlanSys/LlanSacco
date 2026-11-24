using MassTransit.EntityFrameworkCoreIntegration;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LS.Infrastructure.Contracts.Implementations.Common;

public sealed class ContextEventPublisher<TUnitOfWork, TContext>(
    IScopedBusOutbox<TContext> outbox, TContext context,
    ICurrentTenantProvider tenantProvider, ICurrentActorProvider actorProvider)
    : IContextEventPublisher<TUnitOfWork>
    where TUnitOfWork : ITransactionalUnitOfWork
    where TContext : DbContext
{
    private OutboxState? _lastState;

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        var tenantId = tenantProvider.TenantId;

        if (tenantId == Guid.Empty)
        {
            throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        }

        // A UoW retry clears EF tracking after rollback. MassTransit's scoped publisher still
        // references its state entity, so restore only that empty envelope before staging NEW messages.
        // Never restore discarded business writes or outgoing messages from the failed attempt.
        if (_lastState is not null && context.Entry(_lastState).State == EntityState.Detached)
        {
            var exists = await context.Set<OutboxState>().AsNoTracking()
                .AnyAsync(x => x.OutboxId == _lastState.OutboxId, cancellationToken)
                .ConfigureAwait(false);

            context.Entry(_lastState).State = exists ? EntityState.Unchanged : EntityState.Added;
        }
        await outbox.Publish(
            message,
            sendContext =>
            {
                sendContext.Headers.Set("tenant_id", tenantId);
                sendContext.Headers.Set("actor_id", actorProvider.ActorId);
            },
            cancellationToken)
            .ConfigureAwait(false);

        _lastState = context.ChangeTracker.Entries<OutboxState>()
            .FirstOrDefault(x => x.State == EntityState.Added)?
            .Entity ?? _lastState;
    }
}
