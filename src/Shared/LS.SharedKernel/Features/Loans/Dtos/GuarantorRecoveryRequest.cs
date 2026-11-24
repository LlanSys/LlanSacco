namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record GuarantorRecoveryRequest(Guid RecoveryId, decimal Amount, string EvidenceReference);
