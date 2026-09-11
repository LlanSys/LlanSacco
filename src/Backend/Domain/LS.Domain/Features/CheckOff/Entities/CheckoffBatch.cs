using System;
using System.Collections.Generic;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.CheckOff.Enums;

namespace LS.Domain.Features.CheckOff.Entities;

public class CheckoffBatch : BaseEntity
{
    public Guid EmployerId { get; set; }
    public string BatchReference { get; set; } = string.Empty;
    public DateTime ProcessingPeriod { get; set; }
    public DateTimeOffset ReceivedDate { get; set; }
    public decimal TotalAmountReceived { get; set; }
    public CheckoffBatchStatus Status { get; set; }

    // Navigation
    public Employer Employer { get; set; } = null!;
    public ICollection<CheckoffStagingRow> StagingRows { get; set; } = new List<CheckoffStagingRow>();

    public static CheckoffBatch Create(Guid tenantId, Guid employerId, string batchReference, DateTime processingPeriod, decimal totalAmountReceived, string createdBy)
    {
        return new CheckoffBatch
        {
            TenantId = tenantId,
            EmployerId = employerId,
            BatchReference = batchReference,
            ProcessingPeriod = processingPeriod,
            ReceivedDate = DateTimeOffset.UtcNow,
            TotalAmountReceived = totalAmountReceived,
            Status = CheckoffBatchStatus.Staged,
            CreatedBy = createdBy
        };
    }
}
