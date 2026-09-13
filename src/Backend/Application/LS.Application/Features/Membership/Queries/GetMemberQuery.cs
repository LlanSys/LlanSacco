using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Queries;

public record GetMemberQuery(Guid MemberId) : IRequest<AppResponse<MemberResponse>>;

