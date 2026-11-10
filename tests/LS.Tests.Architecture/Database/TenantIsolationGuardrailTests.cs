using System.IO;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

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
                     && !t.Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)
                     && !t.Name.Contains("PostgreSql", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var integrationTestsDir = Path.Combine(AssemblyReferences.RepoRoot, "tests", "LS.Tests.Integration");
        Assert.True(Directory.Exists(integrationTestsDir), $"Could not find Integration Tests directory at {integrationTestsDir}");

        var testFiles = Directory.GetFiles(integrationTestsDir, "*TenantIsolationTests.cs", SearchOption.AllDirectories)
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .ToList();

        var missing = tenantContexts.Where(t => !testFiles.Contains(t.Name.Replace("DBContext", "") + "TenantIsolationTests"));
        Guardrails.ArchitectureDebt.AssertMembers("TENANT001", missing,
            "Tenant-filtered context lacks named tenant-isolation integration test; file presence is not proof of execution", 7);
    }
}
