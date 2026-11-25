using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.IAM.Menus.Queries;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.IAM.DataContext;
using LS.Persistence.Features.IAM.Menus.Repositories;
using LS.Tests.Integration.TestFixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace LS.Tests.Integration;

/// <summary>
/// The navigation module filter runs in SQL, where PostgreSQL string equality is case-sensitive. Tenant modules are
/// stored as "Core" while seeded menus require "core"; the menus must still be returned.
/// </summary>
public sealed class NavigationMenuModuleFilterTests_PostgreSql(PostgreSqlDbFixture fixture) : IClassFixture<PostgreSqlDbFixture>
{
    private sealed class FixedTenant(Guid tenantId) : ICurrentTenantProvider
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class TestActor : ICurrentActorProvider
    {
        public string ActorId => "TestActor";
    }

    private sealed class FixedModules(params string[] modules) : ITenantModuleResolver
    {
        public Task<IReadOnlyList<string>> GetEnabledModulesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(modules);
    }

    [Fact]
    public async Task Navigation_matches_required_modules_case_insensitively()
    {
        var tenantId = Guid.CreateVersion7();
        var options = new DbContextOptionsBuilder<IamDBContext>().UseNpgsql(fixture.GetConnectionString()).Options;
        await using var context = new IamDBContext(options, new FixedTenant(tenantId), new TestActor());
        await context.Database.EnsureCreatedAsync();

        var placement = $"sidebar-{tenantId:N}";
        context.MenuItems.AddRange(
            MenuItem.Create(null, null, $"lower-{tenantId:N}", "Lower", "", "/lower", "icon", placement, null, "core", 1, "test"),
            MenuItem.Create(null, null, $"upper-{tenantId:N}", "Upper", "", "/upper", "icon", placement, null, "CORE", 2, "test"),
            MenuItem.Create(null, null, $"other-{tenantId:N}", "Other", "", "/other", "icon", placement, null, "payroll", 3, "test"),
            MenuItem.Create(null, null, $"open-{tenantId:N}", "Open", "", "/open", "icon", placement, null, null, 4, "test"));
        await context.SaveChangesAsync();

        var unitOfWork = Substitute.For<IIamUnitOfWork>();
        unitOfWork.MenuRepository.Returns(new IamMenuRepository(context));
        var handler = new GetNavigationMenusQueryHandler(
            unitOfWork, new FixedModules("Core"), NullLogger<GetNavigationMenusQueryHandler>.Instance);

        var result = await handler.Handle(new GetNavigationMenusQuery(placement, [], "user", HasFullAccess: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var keys = result.Data!.Select(static menu => menu.Key).ToList();
        Assert.Equal([$"lower-{tenantId:N}", $"upper-{tenantId:N}", $"open-{tenantId:N}"], keys);
    }
}
