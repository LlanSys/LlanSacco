using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Queries;

public record GetMemberQuery(Guid MemberId) : IRequest<AppResponse<MemberResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Member details contain personal data and current membership status.";
}

