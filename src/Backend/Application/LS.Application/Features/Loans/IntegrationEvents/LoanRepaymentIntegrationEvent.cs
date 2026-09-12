using LS.Domain.Shared.Contracts.Common;
using System;

namespace LS.Application.Features.Loans.IntegrationEvents;

public sealed record LoanRepaymentIntegrationEvent(
    Guid TenantId,
    Guid LoanApplicationId,
    Guid RepaymentId,
    decimal Amount,
    decimal PrincipalComponent,
    decimal InterestComponent,
    decimal PenaltyComponent,
    decimal FeeComponent,
    string ReceiptNumber,
    string CreatedBy,
    Guid? BranchId = null,
    Guid? CostCenterId = null,
    Guid? PaymentChannelGlAccountId = null
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
