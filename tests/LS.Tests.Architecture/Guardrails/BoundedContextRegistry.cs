using System.Text.Json;

namespace LS.Tests.Architecture.Guardrails;

internal static class BoundedContextRegistry
{
    internal static string[] Names => JsonSerializer.Deserialize<string[]>(File.ReadAllText(Path.Combine(
        AssemblyReferences.RepoRoot, "docs", "architecture", "bounded-contexts.json")))!;
}
