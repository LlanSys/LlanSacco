using System.Text.Json;

namespace LS.Tests.Architecture.Guardrails;

internal sealed record ReviewedRecordLifecycles(string Entity, string Mode, string Reason)
{
    internal static IReadOnlyDictionary<string, ReviewedRecordLifecycles> Read()
    {
        var path = Path.Combine(AssemblyReferences.RepoRoot, "docs", "architecture", "record-lifecycles.json");
        var records = JsonSerializer.Deserialize<ReviewedRecordLifecycles[]>(File.ReadAllText(path))!;
        Assert.NotEmpty(records);
        Assert.All(
            records,
            record =>
            {
                Assert.StartsWith("LS.Domain.Features.", record.Entity, StringComparison.Ordinal);
                Assert.DoesNotContain('*', record.Entity);
                Assert.True(record.Mode is "FinancialHistory" or "BusinessLifecycle");
                Assert.True(record.Reason.Length >= 40);
            });

        return records.ToDictionary(record => record.Entity, StringComparer.Ordinal);
    }
}
