using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record RevokeUserDeviceCommand(Guid DeviceId, RevokeUserDeviceRequest Request, string RevokedBy)
    : IRequest<AppResponse<bool>>;
