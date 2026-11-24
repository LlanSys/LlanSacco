namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record MyGuarantorPledgeResponse(Guid LoanId, string LoanNumber, Guid PledgeId,
    Guid MemberId, decimal NominatedAmount, decimal AcceptedAmount, decimal LockedAmount,
    string Status, string? FailureReason, string PolicyVersion = "", string BorrowerName = "",
    decimal LoanPrincipal = 0, int LoanTermMonths = 0);
