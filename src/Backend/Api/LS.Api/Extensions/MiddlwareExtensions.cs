using LS.Api.Middleware;

namespace LS.Api.Extensions;

internal static class MiddlwareExtensions
{
    public static IApplicationBuilder UsePostAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder
            .UseMiddleware<SessionValidationMiddleware>()
            .UseMiddleware<MfaEnrollmentMiddleware>();
    }
}
