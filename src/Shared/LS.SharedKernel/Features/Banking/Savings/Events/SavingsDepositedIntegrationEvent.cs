using System;
using MediatR;


namespace LS.SharedKernel.Features.Banking.Savings.Events;

public record SavingsDepositedIntegrationEvent(
    Guid MemberId,
    Guid SavingsAccountId,
    Guid SavingsProductId,
    decimal Amount,
    string ExternalReferenceId
) : INotification
{
    public Guid TenantId { get; init; }
    public Guid TransactionId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

