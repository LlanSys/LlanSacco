using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.ValueObjects;
using MediatR;

namespace LS.Domain.Features.Shared.Payments.Events;

public sealed record PaymentFailedEvent(
    Guid PaymentRecordId,
    string CustomerReference,
    string Provider,
    Money Amount,
    string FailureReason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
