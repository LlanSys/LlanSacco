namespace LS.Tests.Architecture.Guardrails;

public sealed class SourceArchitectureTests
{
    [Fact]
    public void Authored_source_must_not_add_architecture_debt()
    {
        var sources = SourceArchitectureRules.RepositorySources();
        Assert.True(sources.Count > 100, "A missing source inventory must not silently pass.");
        var decisions = System.Text.Json.JsonSerializer.Deserialize<CachePolicyDecision[]>(File.ReadAllText(Path.Combine(
            AssemblyReferences.RepoRoot, "docs", "architecture", "cache-policy-decisions.json")))!;

        Assert.Equal(decisions.Length, decisions.Select(d => d.Symbol).Distinct(StringComparer.Ordinal).Count());
        Assert.All(decisions, d => Assert.True(d.Reason.Length >= 20 && !d.Symbol.Contains('*'), "Cache decisions require an exact symbol and concrete reason."));
        var retainedEntities = ReviewedRecordLifecycles.Read().Keys.ToHashSet(StringComparer.Ordinal);
        var findings = SourceArchitectureRules.Analyze(sources, retainedEntities);
        Assert.All(decisions, d => Assert.Contains(findings, f => f.Rule == "CACHE002" && f.Symbol == d.Symbol));
        findings = findings.Where(f => f.Rule != "CACHE002" || !decisions.Any(d => d.Symbol == f.Symbol)).ToArray();
        findings = ApplyCommandLayoutExceptions(sources, findings);
        var failures = new List<Exception>();

        foreach (var rule in SourceArchitectureRules.RuleIds)
        {
            try
            {
                ArchitectureDebt.AssertKnown(rule, findings.Where(f => f.Rule == rule));
            }
            catch (Exception error)
            {
                failures.Add(error);
            }
        }

        if (failures.Count != 0)
        {
            throw new AggregateException("Architecture drift register mismatch", failures);
        }
    }

    [Fact]
    public void Retained_financial_records_cannot_be_removed_through_a_repository()
    {
        const string source = """
            using LS.Domain.Features.Accounting.Entities;
            using LS.Domain.Shared.Contracts.Repositories;
            namespace LS.Application.Features.Accounting;

            internal sealed class InvalidJournalCleanup(IRepository<Journal> repository)
            {
                internal Task<Journal> RemoveAsync(Guid journalId) => repository.DeleteAsync(journalId);
            }
            """;

        var findings = SourceArchitectureRules.Analyze(
            new Dictionary<string, string> { ["src/Backend/InvalidJournalCleanup.cs"] = source },
            ReviewedRecordLifecycles.Read().Keys.ToHashSet(StringComparer.Ordinal));

        Assert.Contains(findings, finding => finding.Rule == "RET001" && finding.Detail.Contains("Journal", StringComparison.Ordinal));
    }

    private static IReadOnlyList<ArchitectureFinding> ApplyCommandLayoutExceptions(
        IReadOnlyDictionary<string, string> sources,
        IReadOnlyList<ArchitectureFinding> findings)
    {
        var entries = System.Text.Json.JsonSerializer.Deserialize<CommandLayoutException[]>(File.ReadAllText(Path.Combine(
            AssemblyReferences.RepoRoot, "docs", "architecture", "command-layout-exceptions.json")))!;

        Assert.Equal(entries.Length, entries.Select(entry => entry.Symbol).Distinct(StringComparer.Ordinal).Count());
        var infrastructure = typeof(LS.Infrastructure.Extensions.DependencyInjection).Assembly;
        var handlerRequests = AssemblyReferences.Application.GetTypes().Concat(infrastructure.GetTypes())
            .Where(handlerType => !handlerType.IsAbstract)
            .SelectMany(handlerType => handlerType.GetInterfaces()
                .Where(contract => contract.IsGenericType &&
                    contract.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>))
                .Select(contract => (Request: contract.GenericTypeArguments[0], Handler: handlerType)))
            .ToLookup(pair => pair.Request, pair => pair.Handler);

        foreach (var entry in entries)
        {
            Assert.StartsWith("src/Backend/Application/LS.Application/Features/", entry.RequestPath);
            Assert.StartsWith("src/Backend/Infrastructure/LS.Infrastructure/Features/", entry.HandlerPath);
            Assert.False(entry.RequestPath.Contains('*') || entry.HandlerPath.Contains('*'));
            Assert.True(entry.Reason.Length >= 40, "A boundary exception requires a concrete reason.");
            Assert.Single(
                findings,
                finding => finding.Rule == "LAY001" &&
                    finding.Symbol == entry.Symbol && finding.Path == entry.RequestPath);

            var request = AssemblyReferences.Application.GetType(entry.Symbol);
            var handler = infrastructure.GetType(entry.Handler);
            Assert.NotNull(request);
            Assert.NotNull(handler);
            Assert.Equal(handler, Assert.Single(handlerRequests[request]));

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sources[entry.HandlerPath]);
            Assert.Contains(
                tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax>(),
                declaration => declaration.Members.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                    .Any(type => string.Equals($"{declaration.Name}.{type.Identifier.ValueText}", entry.Handler, StringComparison.Ordinal)));
        }

        return findings.Where(finding => finding.Rule != "LAY001" ||
            !entries.Any(entry => entry.Symbol == finding.Symbol && entry.RequestPath == finding.Path))
            .ToArray();
    }

    [Fact]
    public void Valid_operation_pair_is_allowed_but_inline_validator_and_extra_public_type_are_rejected()
    {
        const string command = """
            using MediatR;
            namespace LS.Application.Features.Orders.Orders;
            public record CreateOrderCommand : IRequest<bool>;
            internal class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
            {
                public Task<bool> Handle(CreateOrderCommand request, CancellationToken ct) => Task.FromResult(true);
            }
            """;

        var valid = SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["CreateOrder.cs"] = command });
        Assert.DoesNotContain(valid, f => f.Rule is "LAY001" or "LAY002");
        var invalid = SourceArchitectureRules.Analyze(new Dictionary<string, string>
        {
            ["CreateOrder.cs"] = command + "\ninternal class CreateOrderCommandValidator : FluentValidation.AbstractValidator<CreateOrderCommand> {}\npublic record Unrelated;"
        });

        Assert.Contains(invalid, f => f.Rule == "VAL001");
        Assert.Contains(invalid, f => f.Rule == "LAY002" && f.Symbol.EndsWith("Unrelated", StringComparison.Ordinal));
        Assert.Contains(invalid, f => f.Rule == "LAY001");
    }

    [Fact]
    public void Aliased_raw_repository_and_wrong_suffix_are_rejected()
    {
        const string source = """
            using Raw = LS.Domain.Shared.Contracts.Repositories.IRepository<LS.Domain.Features.Dividends.Entities.DividendDeclaration>;
            namespace Probe;
            public interface IProbeUnitOfWork { Raw Declarations { get; } }
            """;

        var findings = SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["IProbeUnitOfWork.cs"] = source });
        Assert.Contains(findings, f => f.Rule == "UOW001");
        Assert.Contains(findings, f => f.Rule == "UOW002");
    }

    [Fact]
    public void Standalone_internal_validator_is_allowed()
    {
        var findings = SourceArchitectureRules.Analyze(new Dictionary<string, string>
        {
            ["ProbeValidator.cs"] = "internal class ProbeValidator : FluentValidation.AbstractValidator<string> {}"
        });

        Assert.DoesNotContain(findings, f => f.Rule == "VAL001");
    }

    [Fact]
    public void New_and_resolved_debt_are_both_detected_without_treating_line_movement_as_new_debt()
    {
        var original = new ArchitectureFinding("UOW001", "src/Example.cs", "Example.Items", 3, "raw repository", 3);
        var moved = original with { Line = 8 };
        Assert.Empty(ArchitectureDebt.Compare([moved], [original]).Added);
        Assert.Empty(ArchitectureDebt.Compare([moved], [original]).Stale);
        Assert.Single(ArchitectureDebt.Compare([original], []).Added);
        Assert.Single(ArchitectureDebt.Compare([], [original]).Stale);
        Assert.Single(ArchitectureDebt.Compare([original with { Symbol = "Example.MoreItems" }], [original]).Added);
    }

    [Fact]
    public void Handler_dependency_and_errorless_failure_are_detected_semantically()
    {
        const string source = """
            using MediatR;
            using LS.SharedKernel.Dtos.Common;
            using LS.Domain.Shared.Contracts.Repositories;
            using LS.Domain.Features.Dividends.Entities;
            namespace LS.Application.Features.Orders.Orders;
            public record CreateOrderCommand : IRequest<AppResponse<bool>>;
            internal class CreateOrderCommandHandler(IRepository<DividendDeclaration> repository) : IRequestHandler<CreateOrderCommand, AppResponse<bool>>
            {
                public Task<AppResponse<bool>> Handle(CreateOrderCommand request, CancellationToken ct)
                    => Task.FromResult(new AppResponse<bool> { IsSuccess = false, Message = "Rejected" });
            }
            """;

        var findings = SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["CreateOrder.cs"] = source });
        Assert.Contains(findings, f => f.Rule == "DEP001");
        Assert.Contains(findings, f => f.Rule == "ERR001");
        Assert.Contains(findings, f => f.Rule == "CACHE002");
    }

    [Fact]
    public void An_unrelated_handler_cannot_claim_the_operation_file_exception()
    {
        const string source = """
            using MediatR;
            namespace LS.Application.Features.Orders.Orders;
            public record CreateOrderCommand : IRequest<bool>;
            internal class CreateOrderCommandHandler : IRequestHandler<string, bool> {}
            """;

        var findings = SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["CreateOrder.cs"] = source });
        Assert.Contains(findings, f => f.Rule == "LAY001");
        Assert.Contains(findings, f => f.Rule == "LAY002");
    }

    [Fact]
    public void Timestamp_filename_exception_requires_a_real_matching_migration()
    {
        const string migration = "public class AddOrders : Microsoft.EntityFrameworkCore.Migrations.Migration { protected override void Up(Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder b) {} }";
        Assert.DoesNotContain(SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["20260910010101_AddOrders.cs"] = migration }), f => f.Rule == "LAY002");
        Assert.Contains(SourceArchitectureRules.Analyze(new Dictionary<string, string> { ["20260910010101_AddOrders.cs"] = "public class AddOrders {}" }), f => f.Rule == "LAY002");
    }
}
