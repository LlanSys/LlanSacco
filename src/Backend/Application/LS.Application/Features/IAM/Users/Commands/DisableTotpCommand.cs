using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record DisableTotpCommand(string UserId, string DisabledBy)
    : IRequest<AppResponse<bool>>;
