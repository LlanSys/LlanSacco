using System;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Features.Membership.Entities;

namespace LS.Domain.Features.CheckOff.Entities;

public class CheckoffStagingRow : BaseEntity
{
    public Guid CheckoffBatchId { get; set; }
    public required string RawPayrollNumber { get; set; }
    public required string RawMemberName { get; set; }
    public decimal AmountForSavings { get; set; }
    public decimal AmountForShares { get; set; }
    public decimal AmountForLoans { get; set; }
    public decimal TotalDeducted { get; set; }
    public CheckoffRowStatus Status { get; set; }
    public string? ExceptionReason { get; set; }
    public Guid? ResolvedMemberId { get; set; }

    // Navigation
    public CheckoffBatch Batch { get; set; } = null!;
    public Member? ResolvedMember { get; set; }
    public ICollection<CheckoffStagingLoanAllocation> LoanAllocations { get; set; } = new List<CheckoffStagingLoanAllocation>();

    public static CheckoffStagingRow Create(
        Guid tenantId,
        Guid checkoffBatchId,
        string rawPayrollNumber,
        string rawMemberName,
        decimal amountForSavings,
        decimal amountForShares,
        decimal amountForLoans,
        string createdBy)
    {
        return new CheckoffStagingRow
        {
            TenantId = tenantId,
            CheckoffBatchId = checkoffBatchId,
            RawPayrollNumber = rawPayrollNumber,
            RawMemberName = rawMemberName,
            AmountForSavings = amountForSavings,
            AmountForShares = amountForShares,
            AmountForLoans = amountForLoans,
            TotalDeducted = amountForSavings + amountForShares + amountForLoans,
            Status = CheckoffRowStatus.Pending,
            CreatedBy = createdBy
        };
    }
}
