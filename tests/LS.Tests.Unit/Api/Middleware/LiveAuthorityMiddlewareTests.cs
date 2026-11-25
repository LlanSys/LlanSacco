using System.Security.Claims;
using LS.Api.Middleware;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Features.IAM.DataContext;
using LS.SharedKernel.Features.ControlPlane;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Tests.Unit.Api.Middleware;

/// <summary>
/// Authority is re-read from the database on each request, so changes apply before the access token expires,
/// and identity writes stay inside the current tenant.
/// </summary>
public sealed class LiveAuthorityMiddlewareTests : IAsyncDisposable
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private readonly string _databaseName = Guid.CreateVersion7().ToString();
    private readonly IamDBContext _iam;

    public LiveAuthorityMiddlewareTests()
    {
        _iam = new IamDBContext(
            new DbContextOptionsBuilder<IamDBContext>().UseInMemoryDatabase(_databaseName).Options,
            new FixedTenantProvider(TenantId),
            new FixedActorProvider("test-actor"));
    }

    [Fact]
    public async Task Deactivated_account_is_rejected_although_its_token_is_valid()
    {
        var user = await SeedUserAsync(active: false);

        var (http, passed) = await RunAsync(Token(user, permissions: ["members.view"]));

        Assert.False(passed);
        Assert.Equal(StatusCodes.Status401Unauthorized, http.Response.StatusCode);
    }

    [Fact]
    public async Task Permissions_and_roles_come_from_the_database_not_the_token()
    {
        var user = await SeedUserAsync(active: true);
        var role = new AppRole { Id = Guid.CreateVersion7().ToString(), Name = "Teller", NormalizedName = "TELLER" };
        _iam.Roles.Add(role);
        _iam.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        _iam.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = role.Id, ClaimType = "permission", ClaimValue = "teller-tills.manage" });
        _iam.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = role.Id, ClaimType = "permission", ClaimValue = PlatformPermissions.ViewTenants });
        _iam.UserClaims.Add(new IdentityUserClaim<string> { UserId = user.Id, ClaimType = "permission", ClaimValue = PlatformPermissions.ViewStamps });
        await _iam.SaveChangesAsync();

        // The token still claims a revoked permission and role; the database no longer grants them.
        var (http, passed) = await RunAsync(Token(user, permissions: ["members.approve"], roles: ["System Administrator"]));

        Assert.True(passed);
        var permissions = http.User.FindAll("permission").Select(claim => claim.Value).ToHashSet();
        Assert.Contains("teller-tills.manage", permissions);
        Assert.Contains(PlatformPermissions.ViewStamps, permissions);
        Assert.DoesNotContain(PlatformPermissions.ViewTenants, permissions);
        Assert.DoesNotContain("members.approve", permissions);
        Assert.Equal(["Teller"], http.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value));
    }

    [Fact]
    public async Task Password_change_flag_follows_the_account()
    {
        var user = await SeedUserAsync(active: true);

        var (http, _) = await RunAsync(Token(user, permissions: [], passwordChangeRequired: true));

        Assert.False(http.User.HasClaim(claim => claim.Type == "pwd_change_required"));
    }

    [Fact]
    public async Task Identity_write_for_a_user_of_another_tenant_is_refused()
    {
        var foreign = AppUser.CreateSystemUser(Guid.CreateVersion7(), "foreign", "foreign@example.test", "test-actor");
        await SeedForeignUserAsync(foreign);

        _iam.UserClaims.Add(new IdentityUserClaim<string> { UserId = foreign.Id, ClaimType = "permission", ClaimValue = "members.view" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _iam.SaveChangesAsync());
    }

    [Fact]
    public async Task Tenant_scoped_row_of_another_tenant_cannot_be_modified()
    {
        var foreign = AppUser.CreateSystemUser(Guid.CreateVersion7(), "foreign2", "foreign2@example.test", "test-actor");
        await SeedForeignUserAsync(foreign);

        var attached = _iam.Users.IgnoreQueryFilters().Single(user => user.Id == foreign.Id);
        attached.FirstName = "Changed";

        await Assert.ThrowsAsync<InvalidOperationException>(() => _iam.SaveChangesAsync());
    }

    private async Task SeedForeignUserAsync(AppUser foreign)
    {
        // Written through a context of the foreign tenant, as that tenant would.
        await using var other = new IamDBContext(
            new DbContextOptionsBuilder<IamDBContext>().UseInMemoryDatabase(_databaseName).Options,
            new FixedTenantProvider(foreign.TenantId),
            new FixedActorProvider("test-actor"));
        other.Users.Add(foreign);
        await other.SaveChangesAsync();
    }

    private async Task<AppUser> SeedUserAsync(bool active)
    {
        var user = AppUser.CreateSystemUser(TenantId, $"user-{Guid.CreateVersion7():N}", $"{Guid.CreateVersion7():N}@example.test", "test-actor");
        user.IsActive = active;
        user.EmailConfirmed = true;
        user.RequirePasswordChange = false;
        _iam.Users.Add(user);
        await _iam.SaveChangesAsync();
        return user;
    }

    private static ClaimsPrincipal Token(AppUser user, string[] permissions, string[]? roles = null, bool passwordChangeRequired = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new("tenant_id", user.TenantId.ToString()),
            new("mfa_enrolled", "true"),
        };
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
        claims.AddRange((roles ?? []).Select(role => new Claim(ClaimTypes.Role, role)));
        if (passwordChangeRequired)
        {
            claims.Add(new Claim("pwd_change_required", "true"));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme));
    }

    private async Task<(DefaultHttpContext Http, bool Passed)> RunAsync(ClaimsPrincipal principal)
    {
        var http = new DefaultHttpContext
        {
            User = principal,
            RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
        };
        var ticket = new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme);
        http.Features.Set<IAuthenticateResultFeature>(new ResultFeature(AuthenticateResult.Success(ticket)));

        var passed = false;
        await new LiveAuthorityMiddleware(_ => { passed = true; return Task.CompletedTask; }).InvokeAsync(http, _iam);
        return (http, passed);
    }

    public async ValueTask DisposeAsync() => await _iam.DisposeAsync();

    private sealed class ResultFeature(AuthenticateResult result) : IAuthenticateResultFeature
    {
        public AuthenticateResult? AuthenticateResult { get; set; } = result;
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ICurrentTenantProvider
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class FixedActorProvider(string actorId) : ICurrentActorProvider
    {
        public string ActorId { get; } = actorId;
    }
}
