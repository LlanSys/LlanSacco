namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record GuarantorReconciliationResponse(Guid SavingsAccountId, Guid MemberId,
    decimal Balance, decimal LockedFunds, decimal ExplainedReservations, decimal UnexplainedDifference,
    string MemberName = "", string ProductName = "");
