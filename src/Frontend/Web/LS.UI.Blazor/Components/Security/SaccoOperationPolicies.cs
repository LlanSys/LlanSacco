using Microsoft.AspNetCore.Authorization;

namespace LS.UI.Blazor.Components.Security;

public static class SaccoOperationPolicies
{
    public static void Configure(AuthorizationOptions options)
    {
        string[] permissions =
        [
            "accounting-integrations.view", "accounting-integrations.retry",
            "checkoff.view", "checkoff.prepare", "checkoff.post",
            "hr.payroll.view", "hr.payroll.run", "hr.payroll.close", "hr.payroll.settings",
            "applications.create"
        ];

        foreach (var permission in permissions)
        {
            options.AddPolicy(
                permission,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => context.User.IsInRole("System Administrator")
                        || context.User.Claims.Any(claim =>
                            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
                            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
