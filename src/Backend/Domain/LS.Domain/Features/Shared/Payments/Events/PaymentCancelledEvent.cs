using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.ValueObjects;

namespace LS.Domain.Features.Shared.Payments.Events;

public sealed record PaymentCancelledEvent(
    Guid PaymentRecordId,
    string CustomerReference,
    string Provider,
    Money Amount,
    string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
