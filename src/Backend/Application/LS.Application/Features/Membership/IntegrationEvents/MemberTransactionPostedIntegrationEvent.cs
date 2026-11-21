using LS.Domain.Shared.Contracts.Common;
using System;

namespace LS.Application.Features.Membership.IntegrationEvents;

public record MemberTransactionPostedIntegrationEvent(
    Guid TenantId,
    Guid MemberTransactionId,
    Guid MemberAccountId,
    string TransactionType,
    decimal Amount,
    string Reference,
    string? Notes,
    decimal FeeComponent = 0,
    Guid? BranchId = null,
    Guid? CostCenterId = null,
    Guid? PaymentChannelGlAccountId = null) : IIntegrationEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}


