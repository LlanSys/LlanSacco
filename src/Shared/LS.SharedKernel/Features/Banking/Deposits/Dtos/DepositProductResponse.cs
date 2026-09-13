namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record DepositProductResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string Type,
    decimal InterestRate,
    int TermMonths,
    decimal MinimumDeposit,
    string PenaltyStrategy,
    decimal? FlatPenaltyRate,
    decimal? InterestForfeiturePercentage,
    decimal? ProRataReducedInterestRate,
    bool IsActive
);
