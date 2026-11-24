namespace LS.SharedKernel.Features.Loans.Dtos;

public sealed record GuarantorSavingsAccountResponse(Guid Id, string ProductName, decimal AvailableBalance,
    decimal LockedFunds = 0, bool IsActive = true);
