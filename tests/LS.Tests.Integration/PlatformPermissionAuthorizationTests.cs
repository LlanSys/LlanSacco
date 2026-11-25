using System.Security.Claims;
using LS.Api.Common.Authorization;
using LS.Infrastructure.Configuration;
using LS.SharedKernel.Features.ControlPlane;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LS.Tests.Integration;

/// <summary>
/// Platform permissions are honoured only when granted explicitly, to host-tenant staff, on an MFA-enrolled
/// session. A tenant's System Administrator role keeps its tenant-wide bypass but never reaches the platform.
/// </summary>
public sealed class PlatformPermissionAuthorizationTests
{
    private static readonly Guid HostTenant = Guid.Parse("0194f700-0000-7000-8000-000000000001");

    [Fact]
    public async Task Host_staff_with_explicit_permission_and_mfa_is_allowed()
    {
        Assert.True(await Authorize(PlatformPermissions.ViewTenants, User(HostTenant, mfa: true, permissions: [PlatformPermissions.ViewTenants])));
    }

    [Fact]
    public async Task System_administrator_role_does_not_imply_a_platform_permission()
    {
        Assert.False(await Authorize(PlatformPermissions.ViewTenants, User(HostTenant, mfa: true, roles: ["System Administrator"])));
    }

    [Fact]
    public async Task Platform_permission_without_mfa_is_denied()
    {
        Assert.False(await Authorize(PlatformPermissions.ViewTenants, User(HostTenant, mfa: false, permissions: [PlatformPermissions.ViewTenants])));
    }

    [Fact]
    public async Task Platform_permission_held_by_another_tenant_is_denied()
    {
        Assert.False(await Authorize(PlatformPermissions.ViewTenants, User(Guid.CreateVersion7(), mfa: true, permissions: [PlatformPermissions.ViewTenants])));
    }

    [Fact]
    public async Task Retired_coarse_permission_grants_nothing()
    {
        Assert.False(await Authorize(PlatformPermissions.ViewTenants, User(HostTenant, mfa: true, permissions: [PlatformPermissions.LegacyManage])));
    }

    [Fact]
    public async Task Tenant_permissions_keep_the_system_administrator_bypass()
    {
        Assert.True(await Authorize("dividends.approve", User(Guid.CreateVersion7(), mfa: false, roles: ["System Administrator"])));
        Assert.False(await Authorize("dividends.approve", User(Guid.CreateVersion7(), mfa: false)));
    }

    private static async Task<bool> Authorize(string permission, ClaimsPrincipal user)
    {
        var requirement = new PermissionRequirement(permission);
        var context = new AuthorizationHandlerContext([requirement], user, null);
        var handler = new PermissionAuthorizationHandler(Options.Create(new OrgSettings { DefaultTenantId = HostTenant }));
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    private static ClaimsPrincipal User(Guid tenantId, bool mfa, string[]? permissions = null, string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new("tenant_id", tenantId.ToString()),
            new("mfa_enrolled", mfa ? "true" : "false"),
        };
        claims.AddRange((permissions ?? []).Select(permission => new Claim("permission", permission)));
        claims.AddRange((roles ?? []).Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
