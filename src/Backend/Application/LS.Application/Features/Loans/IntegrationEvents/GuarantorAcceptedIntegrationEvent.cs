using LS.Domain.Shared.Contracts.Common;
using System;

namespace LS.Application.Features.Loans.IntegrationEvents;

public sealed record GuarantorAcceptedIntegrationEvent(
    Guid TenantId,
    Guid LoanApplicationId,
    Guid GuarantorMemberId,
    decimal LockedAmount,
    string CreatedBy
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
