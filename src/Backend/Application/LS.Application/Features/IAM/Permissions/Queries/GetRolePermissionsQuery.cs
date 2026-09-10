using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Permissions.Queries;

public sealed record GetRolePermissionsQuery(string RoleId, string UserId)
    : IRequest<AppResponse<RolePermissionsResponse>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";

    public string Discriminator => CacheKeys.Entity("role-permissions", RoleId);

    public string? CacheUserId => null;

    public bool IsVersioned => true;

    public bool BypassCache => true;
}
