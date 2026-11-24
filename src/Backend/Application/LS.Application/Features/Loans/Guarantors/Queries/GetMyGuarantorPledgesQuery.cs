using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

public sealed record GetMyGuarantorPledgesQuery(int Page = 1, int PageSize = 25)
    : IRequest<AppResponse<MyGuarantorPledgesResponse>>, IUncachedQuery
{
    public string CacheBypassReason => "A member must see current consent, funds availability and reservation outcomes.";
}
