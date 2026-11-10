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
        var findings = SourceArchitectureRules.Analyze(sources);
        Assert.All(decisions, d => Assert.Contains(findings, f => f.Rule == "CACHE002" && f.Symbol == d.Symbol));
        findings = findings.Where(f => f.Rule != "CACHE002" || !decisions.Any(d => d.Symbol == f.Symbol)).ToArray();
        var failures = new List<Exception>();
        foreach (var rule in SourceArchitectureRules.RuleIds)
        {
            try { ArchitectureDebt.AssertKnown(rule, findings.Where(f => f.Rule == rule)); }
            catch (Exception error) { failures.Add(error); }
        }
        if (failures.Count != 0) throw new AggregateException("Architecture drift register mismatch", failures);
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
