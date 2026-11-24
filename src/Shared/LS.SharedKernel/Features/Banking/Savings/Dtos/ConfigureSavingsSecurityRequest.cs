namespace LS.SharedKernel.Features.Banking.Savings.Dtos;

public sealed record ConfigureSavingsSecurityRequest(bool AllowsGuarantorReservations,
    bool CountsTowardLoanLimit, string EvidenceReference);
