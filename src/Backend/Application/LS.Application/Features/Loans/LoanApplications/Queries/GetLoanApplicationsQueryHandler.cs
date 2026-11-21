using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.Domain.Features.Loans.Contracts;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Loans.LoanApplications.Queries;

internal sealed class GetLoanApplicationsQueryHandler(ILoansUnitOfWork unitOfWork)
    : IRequestHandler<GetLoanApplicationsQuery, AppResponse<List<LoanApplicationResponse>>>
{
    public async Task<AppResponse<List<LoanApplicationResponse>>> Handle(GetLoanApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await unitOfWork.LoanApplicationRepository.ListAsync(
            q =>
            {
                var query = q.AsQueryable();
                if (request.MemberId.HasValue)
                {
                    query = query.Where(l => l.MemberId == request.MemberId.Value);
                }
                
                return query.OrderByDescending(l => l.ApplicationDate)
                    .Select(a => new LoanApplicationResponse
                    {
                        Id = a.Id,
                        ApplicationNumber = a.ApplicationNumber,
                        MemberId = a.MemberId,
                        LoanProductId = a.LoanProductId,
                        PrincipalAmount = a.PrincipalAmount,
                        TermInMonths = a.TermInMonths,
                        InterestRate = a.InterestRate,
                        Status = a.Status.ToString(),
                        ApplicationDate = a.ApplicationDate,
                        OutstandingPrincipal = a.OutstandingPrincipal,
                        OutstandingInterest = a.OutstandingInterest,
                        Guarantors = a.Guarantors.Select(g => new LoanGuarantorDto
                        {
                            Id = g.Id,
                            GuarantorMemberId = g.GuarantorMemberId,
                            GuaranteedAmount = g.GuaranteedAmount
                        }).ToList()
                    });
            },
            cancellationToken);

        return AppResponses.Success(applications);
    }
}


