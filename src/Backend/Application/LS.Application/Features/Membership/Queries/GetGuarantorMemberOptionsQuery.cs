using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;

namespace LS.Application.Features.Membership.Queries;

public sealed record GetGuarantorMemberOptionsQuery(string Search) : IRequest<AppResponse<List<MemberOptionResponse>>>, IUncachedQuery
{
    public string CacheBypassReason => "Only current active member candidates may be nominated for a guarantee.";
}
