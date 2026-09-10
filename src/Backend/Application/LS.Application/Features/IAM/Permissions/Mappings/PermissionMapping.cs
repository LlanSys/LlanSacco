using LS.Domain.Features.IAM.Permissions.Entities;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;

namespace LS.Application.Features.IAM.Permissions.Mappings;

public static class PermissionMapping
{
    public static PermissionResponse ToPermissionResponse(this Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionResponse(
            permission.Id,
            permission.DepartmentId,
            permission.Key,
            permission.Context,
            permission.Resource,
            permission.Action,
            permission.Description,
            permission.IsActive);
    }
}
