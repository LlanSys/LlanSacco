using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record UpdateRoleCommand(string RoleId, UpdateRoleRequest Request, string UpdatedBy)
    : IRequest<AppResponse<AdminRoleListResponse>>;
