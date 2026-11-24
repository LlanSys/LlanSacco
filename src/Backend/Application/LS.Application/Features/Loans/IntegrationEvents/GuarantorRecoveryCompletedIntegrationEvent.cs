using LS.Domain.Shared.Contracts.Common;

namespace LS.Application.Features.Loans.IntegrationEvents;

public sealed record GuarantorRecoveryCompletedIntegrationEvent(Guid TenantId, Guid LoanApplicationId,
    Guid RecoveryId, decimal Amount, string ReceiptNumber, string CreatedBy) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; }
}
