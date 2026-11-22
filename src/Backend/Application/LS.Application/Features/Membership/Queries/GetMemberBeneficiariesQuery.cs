using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Membership.Queries;

public record GetMemberBeneficiariesQuery(Guid MemberId) : IRequest<AppResponse<List<BeneficiaryResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Beneficiary records contain sensitive member data and require fresh reads.";
}
