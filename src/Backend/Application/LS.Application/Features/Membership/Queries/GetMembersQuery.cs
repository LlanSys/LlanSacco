using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Membership.Queries;

public record GetMembersQuery : IRequest<AppResponse<List<MemberResponse>>>;
