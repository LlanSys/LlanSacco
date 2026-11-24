namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record GuarantorSecurityReviewRequest(Guid ReviewId, Guid PledgeId, string Kind,
    string EvidenceReference, Guid? SavingsAccountId = null, Guid? ReplacementMemberId = null);
