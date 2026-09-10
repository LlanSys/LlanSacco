namespace LS.SharedKernel.Features.Accounting.Dtos;

public record AccountingDashboardStatsResponse(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal NetIncomeYtd,
    int PendingTransactions
);
