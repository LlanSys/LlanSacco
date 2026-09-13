using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LS.Tests.Architecture.Guardrails;

internal static class ArchitectureDebt
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    internal static readonly string[] ReflectionRuleIds = ["EF001", "EF002", "EF003", "EF004", "EF005", "AUTH001", "TENANT001"];
    internal static string RegisterPath => Path.Combine(AssemblyReferences.RepoRoot, "docs", "architecture", "architecture-debt.json");

    internal static void AssertKnown(string rule, IEnumerable<ArchitectureFinding> observed)
    {
        var actual = observed.OrderBy(f => f.Identity, StringComparer.Ordinal).ToArray();
        var artifactDirectory = Path.Combine(AssemblyReferences.RepoRoot, "artifacts", "architecture-inventory");
        Directory.CreateDirectory(artifactDirectory);
        File.WriteAllText(Path.Combine(artifactDirectory, rule + ".json"), JsonSerializer.Serialize(actual, JsonOptions));
        var baseline = JsonSerializer.Deserialize<ArchitectureFinding[]>(File.ReadAllText(RegisterPath))!;
        baseline.Should().NotBeNull();
        baseline.Select(f => f.Identity).Should().OnlyHaveUniqueItems("debt entries identify exact occurrences");
        baseline.Should().OnlyContain(f => SourceArchitectureRules.RuleIds.Contains(f.Rule) || ReflectionRuleIds.Contains(f.Rule), "unknown rules must not become unenforced allowances");
        baseline.Should().OnlyContain(f => f.Phase >= 2 && f.Phase <= 7 && f.Line > 0 &&
            !string.IsNullOrWhiteSpace(f.Symbol) && !string.IsNullOrWhiteSpace(f.Detail) &&
            f.Path.StartsWith("src/", StringComparison.Ordinal) && !f.Path.Contains("..", StringComparison.Ordinal) &&
            !f.Path.Contains('*') && !f.Path.Contains('\\') && !Path.IsPathRooted(f.Path),
            "debt requires an owning phase and exact portable source location");
        var delta = Compare(actual, baseline.Where(f => f.Rule == rule));
        delta.Added.Should().BeEmpty("new architecture drift is forbidden; fix code rather than regenerate the register. Found: {0}", string.Join(Environment.NewLine, delta.Added.Select(f => f.ToString())));
        delta.Stale.Should().BeEmpty("remove resolved entries from the register. Found: {0}", string.Join(Environment.NewLine, delta.Stale.Select(f => f.ToString())));
    }

    internal static (ArchitectureFinding[] Added, ArchitectureFinding[] Stale) Compare(
        IEnumerable<ArchitectureFinding> actual, IEnumerable<ArchitectureFinding> baseline)
    {
        var current = actual.ToDictionary(f => f.Identity, StringComparer.Ordinal);
        var previous = baseline.ToDictionary(f => f.Identity, StringComparer.Ordinal);
        return (current.Values.Where(f => !previous.ContainsKey(f.Identity)).ToArray(),
            previous.Values.Where(f => !current.ContainsKey(f.Identity)).ToArray());
    }

    internal static void AssertMembers(string rule, IEnumerable<MemberInfo> members, string evidence, int phase)
    {
        var declarations = SourceArchitectureRules.RepositorySources().SelectMany(s => CSharpSyntaxTree.ParseText(s.Value)
            .GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Select(t => new
            {
                Path = s.Key, Node = t,
                Name = string.Join(".", t.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString())
                    .Concat(t.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(n => n.Identifier.ValueText)).Append(t.Identifier.ValueText))
            })).ToArray();
        AssertKnown(rule, members.Select(member =>
        {
            var owner = member as Type ?? member.DeclaringType!;
            var declaration = declarations.First(d => d.Name == owner.FullName!.Replace('+', '.'));
            var node = member is MethodInfo method
                ? declaration.Node.DescendantNodes().OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == method.Name)
                : (Microsoft.CodeAnalysis.SyntaxNode)declaration.Node;
            var symbol = member is Type ? owner.FullName! : owner.FullName + "." + member;
            return new ArchitectureFinding(rule, declaration.Path, symbol,
                node.GetLocation().GetLineSpan().StartLinePosition.Line + 1, evidence, phase);
        }));
    }
}
