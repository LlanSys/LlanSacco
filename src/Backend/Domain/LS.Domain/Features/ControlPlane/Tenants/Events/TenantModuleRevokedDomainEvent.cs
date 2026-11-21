using System;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Domain.Features.ControlPlane.Tenants.Events;

public record TenantModuleRevokedDomainEvent(
    Guid TenantId,
    string ModuleKey) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
