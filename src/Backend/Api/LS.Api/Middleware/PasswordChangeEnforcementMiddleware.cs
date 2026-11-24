using LS.Api.Logging;
using LS.Infrastructure.Features.IAM.Users.Security;
using System.Security.Claims;
using System.Text.Json;

namespace LS.Api.Middleware;

/// <summary>
/// Blocks authenticated users whose account still requires a password change (activated accounts and admin-set passwords)
/// from everything except the endpoints needed to complete the change or leave.
/// The flag is read from the <see cref="IamCustomClaimTypes.PasswordChangeRequired"/> access-token claim, which every token
/// issuance path (login, 2FA, passkey, SSO, refresh) derives from <c>AppUser.RequirePasswordChange</c>. After a successful
/// change the client refreshes its token; the refreshed token no longer carries the claim and the gate lifts.
/// </summary>
internal sealed class PasswordChangeEnforcementMiddleware(
    RequestDelegate next,
    ILogger<PasswordChangeEnforcementMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!RequiresPasswordChangeGate(context))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        MiddlewareLogDefinitions.LogPasswordChangeRequired(logger, userId, context.Request.Path.Value ?? string.Empty);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var response = new
        {
            successful = false,
            message = "You must change your password before you can continue.",
            code = "PASSWORD_CHANGE_REQUIRED",
            redirectUrl = "/iam/security"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response)).ConfigureAwait(false);
    }

    private static bool RequiresPasswordChangeGate(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path) || context.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return context.User.HasClaim(
            static claim => string.Equals(claim.Type, IamCustomClaimTypes.PasswordChangeRequired, StringComparison.Ordinal)
                && string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Allowlist: sign-in/out and token refresh, the current user (<c>me</c>), password change and reset, and the
    /// authenticator (TOTP) setup endpoints that <see cref="MfaEnrollmentMiddleware"/> requires users to complete.
    /// </summary>
    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;

        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/login", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/refresh-token", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/logout", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/me", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/password/change", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/auth/password/reset", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/iam/users/totp", StringComparison.OrdinalIgnoreCase);
    }
}
