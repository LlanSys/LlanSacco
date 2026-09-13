using LS.Domain.Shared.Contracts.Common;
using System;

namespace LS.Application.Features.Banking.Shares.IntegrationEvents;

public sealed record SharePurchasedIntegrationEvent(
    Guid TenantId,
    Guid ShareAccountId,
    Guid MemberId,
    int NumberOfShares,
    decimal PricePerShare,
    decimal TotalAmount,
    string? ReferenceNumber,
    string CreatedBy
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
