using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Loans.Guarantors.Entities;

public sealed class GuarantorSecurityReview : BaseEntity, IRetainedFinancialRecord
{
    public Guid LoanId { get; init; }
    public Guid PledgeId { get; init; }
    public Guid? ReplacementPledgeId { get; set; }
    public Guid? ReplacementMemberId { get; init; }
    public Guid? SavingsAccountId { get; init; }
    public required string Kind { get; init; }
    public decimal SourceHeldAmount { get; init; }
    public required string EvidenceReference { get; init; }
    public string Status { get; set; } = "AwaitingApproval";
    public string? ApprovedBy { get; set; }
    public string? FailureReason { get; set; }
}
