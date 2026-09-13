namespace LS.SharedKernel.Features.Loans.Dtos;

public record LoanDashboardStatsResponse(
    int TotalActiveLoans,
    int PendingApplications,
    int LoansInArrears,
    decimal TotalDisbursedAmount
);
