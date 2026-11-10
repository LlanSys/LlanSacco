namespace LS.Tests.Architecture.Guardrails;

public sealed class BoundedContextRegistryTests
{
    [Fact]
    public void Registry_matches_domain_contexts_and_canonical_documentation()
    {
        var names = BoundedContextRegistry.Names;
        Assert.NotEmpty(names);
        Assert.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count());
        var actual = Directory.GetDirectories(Path.Combine(AssemblyReferences.RepoRoot,
            "src", "Backend", "Domain", "LS.Domain", "Features")).Select(Path.GetFileName).Order(StringComparer.Ordinal);
        Assert.Equal(names.Order(StringComparer.Ordinal), actual);
        var contract = File.ReadAllText(Path.Combine(AssemblyReferences.RepoRoot, "AGENTS.md"));
        foreach (var name in names) Assert.Contains($"`{name}`", contract, StringComparison.Ordinal);
    }
}
