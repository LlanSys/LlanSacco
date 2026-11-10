using System;
using MediatR;


namespace LS.SharedKernel.Features.Banking.Savings.Events;

public record SavingsWithdrawnIntegrationEvent(
    Guid MemberId,
    Guid SavingsAccountId,
    Guid SavingsProductId,
    decimal Amount,
    string ExternalReferenceId,
    decimal WithdrawalFee
) : INotification
{
    public Guid TenantId { get; init; }
    public Guid TransactionId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

