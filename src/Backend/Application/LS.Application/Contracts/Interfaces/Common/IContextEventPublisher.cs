using LS.Domain.Shared.Contracts;

namespace LS.Application.Contracts.Interfaces.Common;

public interface IContextEventPublisher<TUnitOfWork> where TUnitOfWork : ITransactionalUnitOfWork
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class;
}
