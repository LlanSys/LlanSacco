using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record CloseMemberCommand(Guid MemberId, string Reason) : IRequest<AppResponse<bool>>;
