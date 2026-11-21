namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record DepositTransactionRequest(
    decimal Amount,
    string? Reference
);
