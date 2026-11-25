using System.Collections.Frozen;

namespace LS.SharedKernel.Features.ControlPlane;

/// <summary>
/// Platform (control-plane) permissions, named as in StarterTemplate. They are honoured only for host-tenant
/// staff with MFA (see <see cref="PlatformAccess"/>); a tenant's own administrators never receive them implicitly.
/// </summary>
public static class PlatformPermissions
{
    /// <summary>Retired coarse permission. Not in <see cref="All"/>, so no policy accepts it.</summary>
    public const string LegacyManage = "control_plane.manage";

    public const string ViewTenants = "control_plane.view_tenants";
    public const string CreateTenants = "control_plane.create_tenants";
    public const string EditTenants = "control_plane.edit_tenants";
    public const string ManageTenantLifecycle = "control_plane.manage_tenant_lifecycle";
    public const string ManageTenantModules = "control_plane.manage_tenant_modules";
    public const string MigrateTenants = "control_plane.migrate_tenants";
    public const string ApproveKyc = "control_plane.approve_kyc";
    public const string ViewStamps = "control_plane.view_stamps";
    public const string ManageStamps = "control_plane.manage_stamps";
    public const string ViewAudit = "control_plane.view_audit";
    public const string ManageSupport = "control_plane.manage_support";

    public static IReadOnlySet<string> All { get; } = new[]
    {
        ViewTenants,
        CreateTenants,
        EditTenants,
        ManageTenantLifecycle,
        ManageTenantModules,
        MigrateTenants,
        ApproveKyc,
        ViewStamps,
        ManageStamps,
        ViewAudit,
        ManageSupport,
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsDefined(string permission) => All.Contains(permission);

    public static bool OpensControlPanel(IEnumerable<string> permissions) => permissions.Any(All.Contains);
}
