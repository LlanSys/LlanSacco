namespace LS.SharedKernel.Features.Loans.Guarantors.Events;

public sealed record GuarantorReservationItem(
    Guid PledgeId, Guid MemberId, Guid SavingsAccountId, decimal Amount,
    string Currency, string PolicyVersion, DateTimeOffset ExpiresAt,
    decimal RecoveryAmount = 0);
