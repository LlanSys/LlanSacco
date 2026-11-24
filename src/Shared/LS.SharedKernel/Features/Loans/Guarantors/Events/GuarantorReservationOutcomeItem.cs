namespace LS.SharedKernel.Features.Loans.Guarantors.Events;

public sealed record GuarantorReservationOutcomeItem(
    Guid PledgeId, Guid SavingsAccountId, decimal Amount, decimal RemainingAmount,
    string State, long Revision);
