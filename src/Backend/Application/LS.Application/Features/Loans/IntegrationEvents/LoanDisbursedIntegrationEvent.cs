using LS.Domain.Shared.Contracts.Common;
using System;

namespace LS.Application.Features.Loans.IntegrationEvents;

public sealed record LoanDisbursedIntegrationEvent(
    Guid TenantId,
    Guid LoanApplicationId,
    Guid MemberId,
    decimal Amount,
    decimal FeeComponent,
    string LoanNumber,
    string CreatedBy,
    Guid? BranchId = null,
    Guid? CostCenterId = null,
    Guid? PaymentChannelGlAccountId = null
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
