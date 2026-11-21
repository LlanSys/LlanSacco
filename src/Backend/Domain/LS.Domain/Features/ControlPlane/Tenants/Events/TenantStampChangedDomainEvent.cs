using System;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.ControlPlane.Tenants.Events;

public record TenantStampChangedDomainEvent(
    Guid TenantId,
    Guid? OldStampId,
    Guid NewStampId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
