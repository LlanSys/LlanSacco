using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Accounting.Entities;

public class AccountingIntegrationError : BaseEntity
{
    public required string EventType { get; set; }
    public required string EventPayload { get; set; }
    public required string ErrorMessage { get; set; }
    public required string Status { get; set; } // "PendingRetry", "Resolved"
    public DateTimeOffset OccurredAt { get; set; }
    public Guid EventId { get; set; }
    
    public static AccountingIntegrationError Create(
        Guid tenantId,
        string eventType,
        string eventPayload,
        string errorMessage,
        string createdBy)
    {
        return new AccountingIntegrationError
        {
            TenantId = tenantId,
            EventType = eventType,
            EventPayload = eventPayload,
            ErrorMessage = errorMessage,
            Status = "PendingRetry",
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy,
            EventId = Guid.Empty // Default or passed in if modified
        };
    }
}
