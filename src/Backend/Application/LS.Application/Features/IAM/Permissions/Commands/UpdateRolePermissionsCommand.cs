using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Permissions.Commands;

public sealed record UpdateRolePermissionsCommand(string RoleId, UpdateRolePermissionsRequest Request, string UserId)
    : IRequest<AppResponse<RolePermissionsResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("role-permissions", RoleId)];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-admin")];
}
