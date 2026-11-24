using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Loans.Guarantors.Entities;

public sealed class GuarantorRecovery : BaseEntity, IRetainedFinancialRecord
{
    public Guid LoanId { get; init; }
    public decimal Amount { get; init; }
    public required string EvidenceReference { get; init; }
    public string? ApprovedBy { get; set; }
    public string Status { get; set; } = "AwaitingApproval";
    public string? FailureReason { get; set; }
}
