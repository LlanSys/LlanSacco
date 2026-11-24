namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record GuarantorOperationResponse(Guid Id, string Kind, string Status, decimal Amount,
    DateTimeOffset CreatedAt, string RequestedBy, string? ApprovedBy, string? FailureReason,
    string EvidenceReference = "", Guid? PledgeId = null, Guid? ReplacementMemberId = null);
