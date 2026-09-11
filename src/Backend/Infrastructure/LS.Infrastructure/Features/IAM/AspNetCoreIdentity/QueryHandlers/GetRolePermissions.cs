using LS.Application.Features.IAM.Permissions.Queries;
using LS.Domain.Features.IAM.Users.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.QueryHandlers;

internal sealed class GetRolePermissions(RoleManager<AppRole> roleManager)
    : IRequestHandler<GetRolePermissionsQuery, AppResponse<RolePermissionsResponse>>
{
    public async Task<AppResponse<RolePermissionsResponse>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId).ConfigureAwait(false);
        if (role is null)
        {
            return AppResponses.Failure<RolePermissionsResponse>("Role not found.");
        }

        var permissionKeys = (await roleManager.GetClaimsAsync(role).ConfigureAwait(false))
            .Where(static claim => claim.Type == "permission")
            .Select(static claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return AppResponses.Success(
            "Role permissions loaded.",
            new RolePermissionsResponse(role.Id, role.Name ?? string.Empty, permissionKeys));
    }
}
