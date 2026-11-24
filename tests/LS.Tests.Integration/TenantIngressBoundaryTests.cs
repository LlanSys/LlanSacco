using System.Linq.Expressions;
using System.Security.Claims;
using LS.Api.Middleware;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LS.Tests.Integration;

/// <summary>
/// The request tenant comes only from an Active tenant host or a valid selector for an Active tenant on this
/// stamp, and a signed-in session can only act in the tenant it was issued for.
/// </summary>
public sealed class TenantIngressBoundaryTests
{
    private const string CentralHost = "api.central.test";

    [Fact]
    public async Task Tenant_host_resolves_its_active_tenant()
    {
        var tenant = NewTenant("alpha.example.test");
        var context = await Resolve(Request(tenant.HostName), tenant);

        Assert.True(context.Passed);
        Assert.Equal(tenant.Id, context.Http.Items[TenantResolutionMiddleware.ResolvedTenantItem]);
        Assert.Equal(tenant.Id.ToString(), context.Http.Request.Headers["X-Tenant-Id"].ToString());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Malformed_selector_is_rejected(string selector)
    {
        var tenant = NewTenant("alpha.example.test");
        var context = await Resolve(Request(CentralHost, selector), tenant);

        Assert.False(context.Passed);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Http.Response.StatusCode);
    }

    [Fact]
    public async Task Selector_conflicting_with_tenant_host_is_rejected()
    {
        var tenant = NewTenant("alpha.example.test");
        var other = NewTenant("beta.example.test");
        var context = await Resolve(Request(tenant.HostName, other.Id.ToString()), tenant, other);

        Assert.False(context.Passed);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Http.Response.StatusCode);
    }

    [Fact]
    public async Task Unknown_host_is_not_served()
    {
        var tenant = NewTenant("alpha.example.test");
        var context = await Resolve(Request("unknown.example.test"), tenant);

        Assert.False(context.Passed);
        Assert.Equal(StatusCodes.Status404NotFound, context.Http.Response.StatusCode);
    }

    [Fact]
    public async Task Central_host_accepts_selector_only_for_an_active_tenant_on_this_stamp()
    {
        var stamp = Guid.CreateVersion7();
        var active = NewTenant("alpha.example.test", stamp);
        var suspended = NewTenant("beta.example.test", stamp, TenantStatus.Suspended);
        var otherStamp = NewTenant("gamma.example.test", Guid.CreateVersion7());

        Assert.True((await Resolve(Request(CentralHost, active.Id.ToString()), stamp, active, suspended, otherStamp)).Passed);

        foreach (var rejected in new[] { suspended.Id, otherStamp.Id, Guid.CreateVersion7() })
        {
            var context = await Resolve(Request(CentralHost, rejected.ToString()), stamp, active, suspended, otherStamp);
            Assert.False(context.Passed);
            Assert.Equal(StatusCodes.Status404NotFound, context.Http.Response.StatusCode);
        }
    }

    [Fact]
    public async Task Central_host_without_selector_passes_without_resolving_a_tenant()
    {
        var context = await Resolve(Request(CentralHost), NewTenant("alpha.example.test"));

        Assert.True(context.Passed);
        Assert.False(context.Http.Items.ContainsKey(TenantResolutionMiddleware.ResolvedTenantItem));
    }

    [Fact]
    public async Task Session_for_another_tenant_is_forbidden()
    {
        var tenant = NewTenant("alpha.example.test");
        var resolved = await Resolve(Request(tenant.HostName), tenant);
        resolved.Http.User = SignedIn(Guid.CreateVersion7());

        var passed = await Boundary(resolved.Http, null, tenant);

        Assert.False(passed);
        Assert.Equal(StatusCodes.Status403Forbidden, resolved.Http.Response.StatusCode);
    }

    [Fact]
    public async Task Session_for_the_resolved_tenant_passes()
    {
        var tenant = NewTenant("alpha.example.test");
        var resolved = await Resolve(Request(tenant.HostName), tenant);
        resolved.Http.User = SignedIn(tenant.Id);

        Assert.True(await Boundary(resolved.Http, null, tenant));
    }

    [Fact]
    public async Task Session_for_a_tenant_not_on_this_stamp_is_forbidden()
    {
        var stamp = Guid.CreateVersion7();
        var elsewhere = NewTenant("alpha.example.test", Guid.CreateVersion7());
        var http = Request(CentralHost);
        http.User = SignedIn(elsewhere.Id);

        Assert.False(await Boundary(http, stamp, elsewhere));
        Assert.Equal(StatusCodes.Status403Forbidden, http.Response.StatusCode);
    }

    internal static ITenantRepository Repository(params Tenant[] tenants)
    {
        var repository = Substitute.For<ITenantRepository>();
        repository.ListAsync(Arg.Any<Func<IQueryable<Tenant>, IQueryable<Tenant>>?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var query = call.Arg<Func<IQueryable<Tenant>, IQueryable<Tenant>>?>();
                var source = tenants.AsQueryable();
                return Task.FromResult((query is null ? source : query(source)).ToList());
            });
        repository.AnyAsync(Arg.Any<Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(tenants.AsQueryable().Any(call.Arg<Expression<Func<Tenant, bool>>>())));
        return repository;
    }

    private static Task<(DefaultHttpContext Http, bool Passed)> Resolve(DefaultHttpContext http, params Tenant[] tenants)
        => Resolve(http, null, tenants);

    private static async Task<(DefaultHttpContext Http, bool Passed)> Resolve(DefaultHttpContext http, Guid? stamp, params Tenant[] tenants)
    {
        var passed = false;
        var middleware = new TenantResolutionMiddleware(_ => { passed = true; return Task.CompletedTask; }, Settings(stamp));
        await middleware.InvokeAsync(http, UnitOfWork(tenants));
        return (http, passed);
    }

    private static async Task<bool> Boundary(DefaultHttpContext http, Guid? stamp, params Tenant[] tenants)
    {
        var passed = false;
        var middleware = new TenantClaimBoundaryMiddleware(_ => { passed = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(http, Settings(stamp), UnitOfWork(tenants));
        return passed;
    }

    private static IControlPlaneUnitOfWork UnitOfWork(Tenant[] tenants)
    {
        var unitOfWork = Substitute.For<IControlPlaneUnitOfWork>();
        var repository = Repository(tenants);
        unitOfWork.TenantRepository.Returns(repository);
        return unitOfWork;
    }

    private static IOptions<OrgSettings> Settings(Guid? stamp) => Options.Create(new OrgSettings
    {
        CentralApiHosts = [CentralHost],
        DeploymentStampId = stamp,
    });

    private static DefaultHttpContext Request(string host, string? selector = null)
    {
        var http = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        http.Request.Host = new HostString(host);
        http.Request.Path = "/api/v1/iam/admin/users";
        if (selector is not null)
        {
            http.Request.Headers["X-Tenant-Id"] = selector;
        }

        return http;
    }

    private static ClaimsPrincipal SignedIn(Guid tenantId)
        => new(new ClaimsIdentity([new Claim("tenant_id", tenantId.ToString())], "test"));

    private static Tenant NewTenant(string host, Guid? stamp = null, TenantStatus status = TenantStatus.Active) => new()
    {
        Identifier = host.Split('.')[0],
        DisplayName = host,
        HostName = host,
        CreatedBy = "ingress-test",
        Status = status,
        DeploymentStampId = stamp ?? Guid.Empty,
    };
}
