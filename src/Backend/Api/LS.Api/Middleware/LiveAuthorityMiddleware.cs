using System.Security.Claims;
using LS.Infrastructure.Features.IAM.Users.Security;
using LS.Persistence.Features.IAM.DataContext;
using LS.SharedKernel.Features.ControlPlane;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace LS.Api.Middleware;

/// <summary>
/// Re-reads the signed-in account on every request and rebuilds its authority claims, so a deactivated account,
/// a revoked role or permission, or a cleared password-change flag takes effect immediately instead of when the
/// access token expires. Ported from StarterTemplate (without its module policy, which LlanSacco does not have).
/// </summary>
internal sealed class LiveAuthorityMiddleware(RequestDelegate next)
{
    private const string PermissionClaim = "permission";

    public async Task InvokeAsync(HttpContext context, IamDBContext iam)
    {
        var ticket = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Ticket;
        if (ticket?.AuthenticationScheme != JwtBearerDefaults.AuthenticationScheme)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var principal = context.User;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(principal.FindFirstValue("tenant_id"), out var tenantId))
        {
            await Unavailable(context).ConfigureAwait(false);
            return;
        }

        var cancellationToken = context.RequestAborted;
        var account = await iam.Users.IgnoreQueryFilters().AsNoTracking()
            .Where(user => user.Id == userId && user.TenantId == tenantId && user.IsActive && !user.IsDeleted)
            .Select(user => new { user.TwoFactorEnabled, user.EmailConfirmed, user.RequirePasswordChange })
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (account is null || !account.EmailConfirmed)
        {
            await Unavailable(context).ConfigureAwait(false);
            return;
        }

        var roles = from membership in iam.UserRoles.AsNoTracking()
                    join role in iam.Roles.AsNoTracking() on membership.RoleId equals role.Id
                    where membership.UserId == userId && role.Name != null
                    select role;
        var roleNames = await roles.Select(role => role.Name!).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var rolePermissions = await (from role in roles
                                     join claim in iam.RoleClaims.AsNoTracking() on role.Id equals claim.RoleId
                                     where claim.ClaimType == PermissionClaim && claim.ClaimValue != null
                                     select claim.ClaimValue!)
            .Distinct().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var directPermissions = await iam.UserClaims.AsNoTracking()
            .Where(claim => claim.UserId == userId && claim.ClaimType == PermissionClaim && claim.ClaimValue != null)
            .Select(claim => claim.ClaimValue!)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        // Platform permissions only ever come from a direct grant; a role (shared by every tenant) never carries one.
        var permissions = directPermissions
            .Concat(rolePermissions.Where(key => !PlatformPermissions.IsDefined(key)))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var claims = principal.Claims.Where(claim => !IsAuthorityClaim(claim.Type)).ToList();
        claims.Add(new Claim(PlatformAccess.MfaEnrolledClaim, account.TwoFactorEnabled ? "true" : "false"));
        if (account.RequirePasswordChange)
        {
            claims.Add(new Claim(IamCustomClaimTypes.PasswordChangeRequired, "true"));
        }

        claims.AddRange(roleNames.Select(name => new Claim(ClaimTypes.Role, name)));
        claims.AddRange(permissions.Select(key => new Claim(PermissionClaim, key)));

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, principal.Identity!.AuthenticationType,
            ClaimTypes.Name, ClaimTypes.Role));
        await next(context).ConfigureAwait(false);
    }

    private static Task Unavailable(HttpContext context)
    {
        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "This account session is unavailable.")
            .ExecuteAsync(context);
    }

    private static bool IsAuthorityClaim(string claimType)
    {
        return string.Equals(claimType, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
            || string.Equals(claimType, "role", StringComparison.OrdinalIgnoreCase)
            || string.Equals(claimType, "roles", StringComparison.OrdinalIgnoreCase)
            || string.Equals(claimType, PermissionClaim, StringComparison.OrdinalIgnoreCase)
            || string.Equals(claimType, PlatformAccess.MfaEnrolledClaim, StringComparison.OrdinalIgnoreCase)
            || string.Equals(claimType, IamCustomClaimTypes.PasswordChangeRequired, StringComparison.OrdinalIgnoreCase);
    }
}
