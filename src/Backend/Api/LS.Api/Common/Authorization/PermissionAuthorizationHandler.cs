using LS.Infrastructure.Configuration;
using LS.SharedKernel.Features.ControlPlane;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LS.Api.Common.Authorization;

internal sealed class PermissionAuthorizationHandler(IOptions<OrgSettings> orgSettings) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        // Platform permissions are never implied by a tenant role: they must be granted explicitly, to staff of
        // the host (platform) tenant, on an MFA-enrolled session.
        if (PlatformPermissions.IsDefined(requirement.Permission))
        {
            if (PlatformAccess.Can(context.User, requirement.Permission)
                && PlatformAccess.IsHostTenant(context.User, orgSettings.Value.DefaultTenantId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        if (context.User.IsInRole(PermissionAuthorizationConstants.SystemAdministratorRole)
            || context.User.Claims.Any(claim =>
                string.Equals(claim.Type, PermissionAuthorizationConstants.ClaimType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(claim.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
