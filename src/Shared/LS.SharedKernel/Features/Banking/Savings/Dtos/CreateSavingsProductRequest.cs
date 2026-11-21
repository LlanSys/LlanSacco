using System;


namespace LS.SharedKernel.Features.Banking.Savings.Dtos;

public record CreateSavingsProductRequest(
    string Name,
    string Code,
    string? Description,
    decimal InterestRate,
    decimal MinimumBalance,
    bool AllowsWithdrawals,
    decimal WithdrawalFee,
    decimal? DailyWithdrawalLimit,
    decimal? MonthlyWithdrawalLimit
);

