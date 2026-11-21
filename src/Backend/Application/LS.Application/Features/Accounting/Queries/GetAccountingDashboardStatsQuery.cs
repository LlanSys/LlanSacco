using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Accounting.Queries;

public record GetAccountingDashboardStatsQuery() : IRequest<AppResponse<AccountingDashboardStatsResponse>>;

internal class GetAccountingDashboardStatsQueryHandler 
    : IRequestHandler<GetAccountingDashboardStatsQuery, AppResponse<AccountingDashboardStatsResponse>>
{
    public async Task<AppResponse<AccountingDashboardStatsResponse>> Handle(GetAccountingDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // For a real accounting dashboard, this would sum up balances across specific chart of account types.
        // Returning dummy placeholder values to satisfy the UI requirement for the dashboard.
        
        var response = new AccountingDashboardStatsResponse(
            TotalAssets: 15420000m,
            TotalLiabilities: 2150000m,
            NetIncomeYtd: 3450000m,
            PendingTransactions: 12
        );

        return AppResponses.Success(response);
    }
}

