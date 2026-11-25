using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LS.Tests.Architecture.Database;

public class TenantIsolationGuardrailTests
{
    [Fact]
    public void TenantFilteredDBContexts_MustHaveCorrespondingIntegrationTest()
    {
        var persistenceAssembly = typeof(LS.Persistence.Features.Shared.DataContext.SharedDBContext).Assembly;
        var tenantFilteredInterface = persistenceAssembly.GetTypes().FirstOrDefault(t => t.Name == "ITenantFilteredDBContext");

        tenantFilteredInterface.Should().NotBeNull("ITenantFilteredDBContext must exist in LS.Persistence.Common");

        var tenantContexts = persistenceAssembly.GetTypes()
            .Where(t => t.IsClass
                && !t.IsAbstract
                && tenantFilteredInterface!.IsAssignableFrom(t)
                && !t.Name.Contains("PostgreSql", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var integrationTestsDir = Path.Combine(AssemblyReferences.RepoRoot, "tests", "LS.Tests.Integration");
        Assert.True(Directory.Exists(integrationTestsDir), $"Could not find Integration Tests directory at {integrationTestsDir}");

        var coveredContexts = Directory.EnumerateFiles(integrationTestsDir, "*TenantIsolationTests.cs", SearchOption.AllDirectories)
            .SelectMany(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Where(method => method.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attribute => attribute.Name.ToString() is "Fact" or "Theory"))
            .SelectMany(method => method.AttributeLists.SelectMany(list => list.Attributes))
            .Select(GetCoveredContext)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        var missing = tenantContexts.Where(context => !coveredContexts.Contains(context.Name));
        Guardrails.ArchitectureDebt.AssertMembers(
            "TENANT001",
            missing,
            "Tenant-filtered context lacks named tenant-isolation integration test; file presence is not proof of execution",
            7);
    }

    private static string? GetCoveredContext(AttributeSyntax attribute)
    {
        if (attribute.Name.ToString() == "Trait"
            && attribute.ArgumentList?.Arguments is
            [
            { Expression: LiteralExpressionSyntax { Token.ValueText: "TenantIsolation" } },
            { Expression: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } contextName }
            ]
            && contextName.ArgumentList.Arguments is [{ Expression: IdentifierNameSyntax context }])
        {
            return context.Identifier.ValueText;
        }

        return null;
    }
}
