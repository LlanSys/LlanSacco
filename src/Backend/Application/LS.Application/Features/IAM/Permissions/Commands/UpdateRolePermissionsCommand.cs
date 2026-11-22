using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Permissions.Commands;

public sealed record UpdateRolePermissionsCommand(string RoleId, UpdateRolePermissionsRequest Request, string UserId)
    : IRequest<AppResponse<RolePermissionsResponse>>;
