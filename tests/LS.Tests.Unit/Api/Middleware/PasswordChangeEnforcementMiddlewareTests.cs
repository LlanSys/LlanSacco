using LS.Api.Middleware;
using LS.Infrastructure.Features.IAM.Users.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using System.Text;

namespace LS.Tests.Unit.Api.Middleware;

public sealed class PasswordChangeEnforcementMiddlewareTests
{
    private const string UserId = "019e012a-59c7-7abc-bf6a-46e4934515b5";

    [Theory]
    [InlineData("/api/v1/members")]
    [InlineData("/api/v1/iam/admin/users")]
    [InlineData("/api/v1/iam/auth/passkeys")]
    [InlineData("/hubs/notifications/negotiate")]
    public async Task Flagged_user_is_blocked_with_password_change_required(string path)
    {
        var (middleware, nextCalled) = CreateMiddleware();
        var context = CreateContext(path, flagged: true);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled());
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Contains("PASSWORD_CHANGE_REQUIRED", await ReadResponseAsync(context), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/v1/iam/auth/password/change")]
    [InlineData("/api/v1/iam/auth/password/reset")]
    [InlineData("/api/v1/iam/auth/logout")]
    [InlineData("/api/v1/iam/auth/refresh-token")]
    [InlineData("/api/v1/iam/auth/me")]
    [InlineData("/api/v1/iam/auth/login")]
    [InlineData("/api/v1/iam/users/totp/setup")]
    [InlineData("/health")]
    public async Task Flagged_user_can_reach_allowlisted_endpoints(string path)
    {
        var (middleware, nextCalled) = CreateMiddleware();
        var context = CreateContext(path, flagged: true);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled());
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Unflagged_user_passes()
    {
        var (middleware, nextCalled) = CreateMiddleware();
        var context = CreateContext("/api/v1/members", flagged: false);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled());
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_request_passes_to_normal_authorization()
    {
        var (middleware, nextCalled) = CreateMiddleware();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/members";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled());
    }

    private static (PasswordChangeEnforcementMiddleware Middleware, Func<bool> NextCalled) CreateMiddleware()
    {
        var called = false;
        var middleware = new PasswordChangeEnforcementMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            NullLogger<PasswordChangeEnforcementMiddleware>.Instance);

        return (middleware, () => called);
    }

    private static DefaultHttpContext CreateContext(string path, bool flagged)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, UserId) };
        if (flagged)
        {
            claims.Add(new Claim(IamCustomClaimTypes.PasswordChangeRequired, "true"));
        }

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "unit-test"));
        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
