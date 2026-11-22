using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record UpdateUserPermissionsCommand(string UserId, UpdateUserPermissionsRequest Request, string UpdatedBy)
    : IRequest<AppResponse<UserPermissionsResponse>>;
