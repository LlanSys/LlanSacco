using LS.Domain.Shared.Entities;
using LS.Domain.Features.Accounting.Enums;
using System;
using System.Collections.Generic;

namespace LS.Domain.Features.Accounting.Entities;

public class Journal : BaseEntity
{
    public required string ReferenceNumber { get; set; }
    public required string Description { get; set; }
    public JournalStatus Status { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    
    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();

    public static Journal Create(
        Guid tenantId,
        string referenceNumber,
        string description,
        DateTimeOffset transactionDate,
        string createdBy)
    {
        return new Journal
        {
            TenantId = tenantId,
            ReferenceNumber = referenceNumber,
            Description = description,
            Status = JournalStatus.Draft,
            TransactionDate = transactionDate,
            CreatedBy = createdBy
        };
    }
}
