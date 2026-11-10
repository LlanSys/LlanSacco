using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Membership.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record OnboardMemberCommand(OnboardMemberRequest Request) : IRequest<AppResponse<Guid>>;
