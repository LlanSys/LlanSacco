namespace LS.SharedKernel.Features.IAM.Menus;

/// <summary>
/// Operation floors for shipped administration pages, independent of configurable menu visibility (ported from
/// StarterTemplate). A menu entry for one of these pages is only shown to users who can read what the page shows.
/// </summary>
/// <remarks>
/// LS differences: Departments and Employees are LS administration pages. Org settings and support have no floor
/// because LS has no <c>tenant-settings.view</c> or <c>support.approve</c> permission yet.
/// </remarks>
public static class AdministrationPageAccess
{
    public static IReadOnlyList<string> PermissionKeys { get; } =
    [
        "menus.view", "roles.view", "roles.manage_permissions", "users.view", "departments.view", "employees.view"
    ];

    public static bool CanOpen(string path, IEnumerable<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(permissions);
        var keys = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var route = path.Split('?', '#')[0].TrimEnd('/').ToUpperInvariant();
        return route switch
        {
            "/ADMIN" => PermissionKeys.Any(keys.Contains),
            "/ADMIN/MENUS" or "/ADMIN/ACCESS-CATALOG" or "/ADMIN/SETTINGS" => keys.Contains("menus.view"),
            "/ADMIN/IAM" => keys.Contains("roles.view") || keys.Contains("roles.manage_permissions") || keys.Contains("users.view"),
            "/ADMIN/DEPARTMENTS" => keys.Contains("departments.view"),
            "/ADMIN/EMPLOYEES" => keys.Contains("employees.view"),
            _ => true
        };
    }
}
