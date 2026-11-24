namespace LS.SharedKernel.Features.Loans.Guarantors.Events;

public sealed record GuarantorReservationInstruction(
    Guid TenantId, Guid OperationId, Guid LoanId, string Operation,
    IReadOnlyList<GuarantorReservationItem> Items, string RequestedBy,
    decimal RequiredCoverage = 0, bool ReleaseCommitted = false,
    string? ApprovedBy = null, string? EvidenceReference = null)
{
    public int FormatVersion { get; init; } = 1;
}
