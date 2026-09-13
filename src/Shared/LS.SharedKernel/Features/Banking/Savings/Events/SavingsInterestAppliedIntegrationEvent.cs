using System;
using MediatR;


namespace LS.SharedKernel.Features.Banking.Savings.Events;

public record SavingsInterestAppliedIntegrationEvent(
    Guid MemberId,
    Guid SavingsAccountId,
    Guid SavingsProductId,
    decimal Amount
) : INotification
{
    public Guid TenantId { get; init; }
    public Guid TransactionId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
