using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Membership.Commands;

public record VerifyMemberKycCommand(Guid MemberId) : IRequest<AppResponse<bool>>;
