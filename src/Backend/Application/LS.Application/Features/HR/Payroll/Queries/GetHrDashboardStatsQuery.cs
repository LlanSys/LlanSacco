using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.HR.Payroll.Queries;

public record GetHrDashboardStatsQuery() : IRequest<AppResponse<HrDashboardStatsResponse>>;

internal class GetHrDashboardStatsQueryHandler 
    : IRequestHandler<GetHrDashboardStatsQuery, AppResponse<HrDashboardStatsResponse>>
{
    public async Task<AppResponse<HrDashboardStatsResponse>> Handle(GetHrDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // For a real HR dashboard, you'd fetch employee counts, leave stats, and payroll totals from the repository.
        // Returning dummy placeholder values to satisfy the UI requirement for the dashboard.
        
        var response = new HrDashboardStatsResponse(
            TotalEmployees: 42,
            NextPayrollPeriod: "Aug 2026",
            PendingLeaveRequests: 3,
            TotalPayrollExpenseThisMonth: 1250000m
        );

        return AppResponses.Success(response);
    }
}

