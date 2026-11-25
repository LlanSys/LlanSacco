using System.Security.Claims;

namespace LS.SharedKernel.Features.ControlPlane;

/// <summary>
/// Platform access rules shared by the API and the UI. A platform permission counts only when it is granted
/// explicitly and the session is MFA-enrolled; the API additionally requires the host (platform) tenant.
/// </summary>
public static class PlatformAccess
{
    public const string PermissionClaim = "permission";
    public const string MfaEnrolledClaim = "mfa_enrolled";
    public const string TenantClaim = "tenant_id";

    public static bool HasMfa(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return string.Equals(user.FindFirst(MfaEnrolledClaim)?.Value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public static bool Can(ClaimsPrincipal user, string permission)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.Identity?.IsAuthenticated == true
            && PlatformPermissions.IsDefined(permission)
            && HasMfa(user)
            && user.FindAll(PermissionClaim).Any(claim => string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    public static bool CanOpenControlPanel(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.Identity?.IsAuthenticated == true
            && HasMfa(user)
            && PlatformPermissions.OpensControlPanel(user.FindAll(PermissionClaim).Select(claim => claim.Value));
    }

    public static bool IsHostTenant(ClaimsPrincipal user, Guid hostTenantId)
    {
        ArgumentNullException.ThrowIfNull(user);
        return hostTenantId != Guid.Empty
            && Guid.TryParse(user.FindFirst(TenantClaim)?.Value, out var tenantId)
            && tenantId == hostTenantId;
    }
}
