using LS.Domain.Shared.Contracts.Common;

namespace LS.Persistence.Common;

internal interface IDomainEventBuffer
{
    IReadOnlyList<IDomainEvent>? GetCollectedDomainEvents();
    void ClearCollectedDomainEvents();
}
