using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Domain.Features.Loans.Contracts.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LS.Application.Features.Loans.LoanApplications.Queries;

public record GetLoanDashboardStatsQuery() : IRequest<AppResponse<LoanDashboardStatsResponse>>;

internal class GetLoanDashboardStatsQueryHandler(ILoanApplicationRepository loanRepository) 
    : IRequestHandler<GetLoanDashboardStatsQuery, AppResponse<LoanDashboardStatsResponse>>
{
    public async Task<AppResponse<LoanDashboardStatsResponse>> Handle(GetLoanDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var activeLoans = await loanRepository.CountAsync(
            l => l.Status == Domain.Features.Loans.Enums.LoanStatus.Active || l.Status == Domain.Features.Loans.Enums.LoanStatus.Arrears, 
            cancellationToken);
            
        var pendingApplications = await loanRepository.CountAsync(
            l => l.Status == Domain.Features.Loans.Enums.LoanStatus.UnderReview || l.Status == Domain.Features.Loans.Enums.LoanStatus.Submitted, 
            cancellationToken);
            
        var loansInArrears = await loanRepository.CountAsync(
            l => l.Status == Domain.Features.Loans.Enums.LoanStatus.Arrears, 
            cancellationToken);
            
        // Calculate disbursed amount using a generic repository doesn't easily support SumAsync natively without an IQueryable interface.
        // For dashboard purposes in this demo, we'll return a placeholder or calculate if we had a dedicated query.
        decimal totalDisbursed = 0; // Requires a custom DB query for performance in a real app.

        var response = new LoanDashboardStatsResponse(
            TotalActiveLoans: activeLoans,
            PendingApplications: pendingApplications,
            LoansInArrears: loansInArrears,
            TotalDisbursedAmount: totalDisbursed
        );

        return AppResponses.Success(response);
    }
}

