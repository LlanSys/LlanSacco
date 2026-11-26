using MudBlazor;
using System.Reflection;

namespace LS.UI.Blazor.Components.Common;

internal static class MenuIconMapper
{
    private static readonly Lazy<Dictionary<string, string>> MaterialFilledIcons = new(BuildMaterialFilledIconMap);

    public static IReadOnlyList<MenuIconOption> Options { get; } =
    [
        new("AccountTree", "Account tree", Icons.Material.Filled.AccountTree),
        new("AdminPanelSettings", "Admin panel", Icons.Material.Filled.AdminPanelSettings),
        new("AutoStories", "Story/book", Icons.Material.Filled.AutoStories),
        new("Badge", "Badge", Icons.Material.Filled.Badge),
        new("Business", "Business", Icons.Material.Filled.Business),
        new("Dashboard", "Dashboard", Icons.Material.Filled.Dashboard),
        new("Devices", "Devices", Icons.Material.Filled.Devices),
        new("Group", "Group", Icons.Material.Filled.Group),
        new("LockPerson", "Security lock", Icons.Material.Filled.LockPerson),
        new("Menu", "Generic menu", Icons.Material.Filled.Menu),
        new("MenuOpen", "Menu", Icons.Material.Filled.MenuOpen),
        new("Settings", "Settings", Icons.Material.Filled.Settings)
    ];

    public static string Resolve(string? icon)
    {
        var key = icon?.Trim();
        return string.IsNullOrWhiteSpace(key)
            ? Icons.Material.Filled.Menu
            : Options.FirstOrDefault(option => option.Key.Equals(key, StringComparison.OrdinalIgnoreCase))?.Icon
            ?? (MaterialFilledIcons.Value.TryGetValue(key, out var resolvedIcon) ? resolvedIcon : Icons.Material.Filled.Menu);
    }

    private static Dictionary<string, string> BuildMaterialFilledIconMap()
    {
        // MudBlazor declares its icons as public const string fields, not properties. Reading properties found
        // none, so every icon outside Options fell back to the generic Menu icon.
        // Some names differ only by case (AddChart / Addchart), so keep the first and skip case-only duplicates
        // instead of letting ToDictionary throw and break every component that renders an icon.
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in typeof(Icons.Material.Filled).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(string) && field.GetValue(null) is string { Length: > 0 } icon)
            {
                map.TryAdd(field.Name, icon);
            }
        }

        return map;
    }
}

internal sealed record MenuIconOption(string Key, string Label, string Icon);
