using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts;

namespace LS.Infrastructure.Contracts.Implementations.Common;

public sealed class DisabledContextEventPublisher<TUnitOfWork> : IContextEventPublisher<TUnitOfWork>
    where TUnitOfWork : ITransactionalUnitOfWork
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
        => throw new InvalidOperationException("Transactional messaging must be enabled before financial posting is available.");
}
