using System;


namespace LS.SharedKernel.Features.Banking.Savings.Dtos;

public record WithdrawSavingsRequest(
    Guid MemberId,
    Guid SavingsProductId,
    decimal Amount,
    string? Notes,
    string? ExternalReferenceId
);
