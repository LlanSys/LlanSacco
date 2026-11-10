using System;


namespace LS.SharedKernel.Features.Banking.Savings.Dtos;

public record SavingsAccountResponse(
    Guid Id,
    Guid MemberId,
    Guid SavingsProductId,
    string ProductName,
    decimal Balance,
    decimal LockedFunds,
    decimal AvailableBalance,
    bool IsActive,
    DateTimeOffset OpenedAt
);

