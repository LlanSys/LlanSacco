namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record HrDashboardStatsResponse(
    int TotalEmployees,
    string NextPayrollPeriod,
    int PendingLeaveRequests,
    decimal TotalPayrollExpenseThisMonth
);
