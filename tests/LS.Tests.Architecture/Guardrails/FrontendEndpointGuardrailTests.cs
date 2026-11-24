using System.Text.RegularExpressions;
using Xunit;

namespace LS.Tests.Architecture.Guardrails;

/// <summary>
/// Every API path the Blazor app is configured to call must match a real API route. A drifted path
/// fails silently at runtime (for example, change-password pointed at a non-existent route, so the
/// password-change screen could never succeed).
/// </summary>
[Trait("Category", "Guardrail")]
public sealed partial class FrontendEndpointGuardrailTests
{
    /// <summary>
    /// Known client endpoints with no API implementation yet. Remove an entry when its API lands;
    /// never add one to hide a typo.
    /// </summary>
    private static readonly HashSet<string> KnownMissingApis = new(StringComparer.Ordinal)
    {
        "LookupCatalogTypes", "LookupByType", "LookupCreate", "LookupUpdate", "LookupDelete",
        "RegisterMpesaC2BUrls", "SimulateMpesaC2B",
        "Search", "Detail", "Create", "Update", "Delete",
    };

    [Fact]
    public void Configured_frontend_endpoints_match_api_routes()
    {
        var root = FindRepositoryRoot();
        var routes = ApiRoutes(Path.Combine(root, "src", "Backend", "Api", "LS.Api"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "Frontend", "Web", "LS.UI.Blazor", "Configuration", "BackendApiSettings.cs"));

        var unmatched = EndpointSetting().Matches(settings)
            .Select(match => (Name: match.Groups["name"].Value, Path: match.Groups["path"].Value))
            .Where(endpoint => !routes.Contains(Normalize(endpoint.Path.Split('?')[0])))
            .Where(endpoint => !KnownMissingApis.Contains(endpoint.Name))
            .Select(endpoint => $"{endpoint.Name} -> {endpoint.Path}")
            .ToList();

        Assert.True(unmatched.Count == 0, "Frontend endpoints without an API route:\n" + string.Join('\n', unmatched));
    }

    private static HashSet<string> ApiRoutes(string apiRoot)
    {
        var routes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(apiRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            var controllerRoute = ControllerRoute().Match(source);
            var basePath = controllerRoute.Success ? controllerRoute.Groups["route"].Value : string.Empty;

            foreach (Match action in ActionRoute().Matches(source))
            {
                var template = action.Groups["template"].Value;
                routes.Add(Normalize(template.Length == 0 ? basePath : $"{basePath}/{template}"));
            }
        }

        return routes;
    }

    private static string Normalize(string path)
    {
        var value = path.Trim('/').ToLowerInvariant();
        value = VersionToken().Replace(value, "VER");
        return RouteToken().Replace(value, "{x}");
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

    [GeneratedRegex("""public string (?<name>\w+) \{ get; init; \} = "(?<path>api/[^"]+)";""")]
    private static partial Regex EndpointSetting();

    [GeneratedRegex("""\[Route\("(?<route>[^"]+)"\)\]""")]
    private static partial Regex ControllerRoute();

    [GeneratedRegex("""\[Http(?:Get|Post|Put|Delete|Patch)(?:\("(?<template>[^"]*)"\))?\]""")]
    private static partial Regex ActionRoute();

    [GeneratedRegex(@"\{version[^}]*\}")]
    private static partial Regex VersionToken();

    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex RouteToken();
}
