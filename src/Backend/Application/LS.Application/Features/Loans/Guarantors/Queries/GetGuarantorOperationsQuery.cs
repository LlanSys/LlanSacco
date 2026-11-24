using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

public sealed record GetGuarantorOperationsQuery(Guid LoanId) : IRequest<AppResponse<List<GuarantorOperationResponse>>>, IUncachedQuery
{
    public string CacheBypassReason => "Security approvals, recovery outcomes and pending financial work must be current.";
}
