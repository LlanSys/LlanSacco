using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Infrastructure.Contracts.Implementations.Common;

internal sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TIntegrationEvent : IIntegrationEvent
        => Task.CompletedTask;
}
