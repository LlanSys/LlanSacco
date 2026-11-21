namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record CreateDepositProductRequest(
    string Name,
    string Code,
    string? Description,
    string Type, // DepositType enum as string
    decimal InterestRate,
    int TermMonths,
    decimal MinimumDeposit,
    string PenaltyStrategy, // EarlyWithdrawalPenaltyStrategy enum as string
    decimal? FlatPenaltyRate,
    decimal? InterestForfeiturePercentage,
    decimal? ProRataReducedInterestRate
);
