using System.Text.RegularExpressions;
using Xunit;

namespace LS.Tests.Architecture.Guardrails;

/// <summary>
/// The coarse control-plane permission was retired in favour of the granular <c>PlatformPermissions</c>
/// catalog. Its only allowed mention is the <c>LegacyManage</c> constant that documents it.
/// </summary>
[Trait("Category", "Guardrail")]
public sealed partial class PlatformPermissionGuardrailTests
{
    [Fact]
    public void Retired_control_plane_permission_is_not_used()
    {
        var root = FindRepositoryRoot();
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".cs", StringComparison.Ordinal) || file.EndsWith(".razor", StringComparison.Ordinal))
            .Where(file => !IsExcluded(file))
            .Where(file => RetiredPermission().IsMatch(File.ReadAllText(file)))
            .Select(file => Path.GetRelativePath(root, file))
            .ToList();

        Assert.True(offenders.Count == 0, "Retired control-plane permission still referenced in:\n" + string.Join('\n', offenders));
    }

    private static bool IsExcluded(string file)
    {
        var separator = Path.DirectorySeparatorChar;
        return file.Contains($"{separator}bin{separator}", StringComparison.Ordinal)
            || file.Contains($"{separator}obj{separator}", StringComparison.Ordinal)
            || file.Contains($"{separator}Migrations{separator}", StringComparison.Ordinal)
            || file.EndsWith($"ControlPlane{separator}PlatformPermissions.cs", StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlanSacco.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }

    [GeneratedRegex(@"control_?plane\.manage\b|""ControlPlane\.Manage""", RegexOptions.IgnoreCase)]
    private static partial Regex RetiredPermission();
}
