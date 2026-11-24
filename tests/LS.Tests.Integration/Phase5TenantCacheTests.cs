using System.Linq.Expressions;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LS.Tests.Integration;

public sealed class Phase5TenantCacheTests
{
    [Fact]
    public async Task New_requests_observe_tenant_suspension_immediately()
    {
        var tenant = NewTenant();
        var repository = Substitute.For<ITenantRepository>();
        repository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult<Tenant?>(call.Arg<Expression<Func<Tenant, bool>>>().Compile()(tenant) ? tenant : null));

        await using var services = Services(repository);
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);
        var activeRequest = new DefaultHttpContext();
        activeRequest.Request.Host = new HostString(tenant.HostName);

        await middleware.InvokeAsync(activeRequest, services);
        Assert.Equal(tenant.Id.ToString(), activeRequest.Request.Headers["X-Tenant-Id"].ToString());

        tenant.Status = TenantStatus.Suspended;
        var nextRequest = new DefaultHttpContext();
        nextRequest.Request.Host = activeRequest.Request.Host;
        await middleware.InvokeAsync(nextRequest, services);

        Assert.False(nextRequest.Request.Headers.ContainsKey("X-Tenant-Id"));
    }

    [Fact]
    public async Task A_new_routing_scope_observes_changes_while_existing_connections_use_a_consistent_snapshot()
    {
        var tenant = NewTenant();
        tenant.DatabaseConnectionString = "first-route";
        tenant.DatabaseProvider = "PostgreSql";
        var repository = Substitute.For<ITenantRepository>();
        repository.FindByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Tenant?>(tenant));
        await using var services = Services(repository);
        var tenantProvider = Substitute.For<ICurrentTenantProvider>();
        tenantProvider.TenantId.Returns(tenant.Id);
        var encryption = Substitute.For<IEncryptionService>();
        encryption.Decrypt(Arg.Any<string>()).Returns(call => call.Arg<string>());
        var current = new TenantConnectionResolver(tenantProvider, services, encryption);

        Assert.Equal("first-route", await current.GetConnectionStringAsync());
        tenant.DatabaseConnectionString = "second-route";
        tenant.DatabaseProvider = "PostgreSql";

        Assert.Equal("PostgreSql", await current.GetDatabaseProviderAsync());
        Assert.Equal("first-route", await current.GetConnectionStringAsync());

        var next = new TenantConnectionResolver(tenantProvider, services, encryption);
        Assert.Equal("second-route", await next.GetConnectionStringAsync());
        Assert.Equal("PostgreSql", await next.GetDatabaseProviderAsync());
        await repository.Received(2).FindByIdAsync(tenant.Id, Arg.Any<CancellationToken>());
    }

    private static ServiceProvider Services(ITenantRepository repository)
    {
        var unitOfWork = Substitute.For<IControlPlaneUnitOfWork>();
        unitOfWork.TenantRepository.Returns(repository);
        var services = new ServiceCollection();
        services.AddSingleton(unitOfWork);
        services.AddOptions<OrgSettings>();
        return services.BuildServiceProvider();
    }

    private static Tenant NewTenant() => new()
    {
        Identifier = "tenant-test",
        DisplayName = "Tenant test",
        HostName = "tenant-test.example.test",
        CreatedBy = "phase5-test",
        Status = TenantStatus.Active
    };
}
