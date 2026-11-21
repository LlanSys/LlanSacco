using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.ValueObjects;
using MediatR;

namespace LS.Domain.Features.Shared.Payments.Events;

public sealed record PaymentCompletedEvent(
    Guid PaymentRecordId,
    string CustomerReference,
    string Provider,
    Money Amount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
