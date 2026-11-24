namespace LS.SharedKernel.Features.Loans.Guarantors.Events;

public sealed record GuarantorReservationOutcome(
    Guid TenantId, Guid OperationId, Guid LoanId, string Operation,
    bool Succeeded, string? Reason,
    IReadOnlyList<GuarantorReservationOutcomeItem> Items);
