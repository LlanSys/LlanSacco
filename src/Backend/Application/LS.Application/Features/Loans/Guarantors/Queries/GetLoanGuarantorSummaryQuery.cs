using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

public sealed record GetLoanGuarantorSummaryQuery(Guid LoanId) : IRequest<AppResponse<LoanApplicationResponse>>, IUncachedQuery
{
    public string CacheBypassReason => "Credit staff must see current principal, pending recoveries and guarantor decisions.";
}
