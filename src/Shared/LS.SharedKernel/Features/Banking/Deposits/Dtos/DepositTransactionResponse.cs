namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record DepositTransactionResponse(
    Guid Id,
    Guid DepositAccountId,
    string Type, // DepositTransactionType as string
    decimal Amount,
    decimal BalanceAfter,
    string? Reference,
    DateTimeOffset CreatedAt
);
