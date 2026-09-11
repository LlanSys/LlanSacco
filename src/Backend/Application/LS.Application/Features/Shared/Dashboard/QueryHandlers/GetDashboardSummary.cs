using LS.Application.Utilities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Dashboard.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Shared.Dashboard.QueryHandlers;

/// <summary>
/// Placeholder dashboard handler — will be rebuilt for Membership/Accounting/Loans data.
/// The original Customer-based dashboard was retired along with the Banking bounded context.
/// </summary>
internal sealed class GetDashboardSummaryQueryHandler(ILogger<GetDashboardSummaryQueryHandler> _logger)
    : IRequestHandler<GetDashboardSummaryQuery, AppResponse<DashboardSummaryResponse>>
{
    public Task<AppResponse<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery query, CancellationToken ct)
    {
        // TODO: Rebuild dashboard with Membership/Accounting/Loans aggregate data
        var response = new DashboardSummaryResponse(
            TotalCustomers: 0,
            ActiveCustomers: 0,
            PendingApprovalCustomers: 0,
            DraftCustomers: 0,
            BySegment: [],
            ByCustomerType: [],
            Aging: new AgingData(
                new AgingBucket(0, 0, 0, 0, 0, 0),
                new AgingBucket(0, 0, 0, 0, 0, 0)),
            RmWorkload: []);

        return Task.FromResult(AppResponses.Success(response));
    }
}
