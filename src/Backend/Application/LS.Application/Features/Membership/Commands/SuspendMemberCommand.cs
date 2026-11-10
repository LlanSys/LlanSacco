using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record SuspendMemberCommand(Guid MemberId, string Reason) : IRequest<AppResponse<bool>>;
