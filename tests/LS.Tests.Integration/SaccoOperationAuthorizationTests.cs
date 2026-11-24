extern alias Blazor;

using Blazor::LS.UI.Blazor.Components.Security;
using LS.Api.Common.Authorization;
using LS.Api.Features.Accounting.Controllers;
using LS.Api.Features.CheckOff.Controllers;
using LS.Api.Features.HR.Payroll.Controllers;
using LS.Api.Features.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Claims;

namespace LS.Tests.Integration;

public sealed class SaccoOperationAuthorizationTests
{
    [Theory]
    [InlineData("BatchesList", typeof(CheckOffController), nameof(CheckOffController.GetBatches), "checkoff.view", "checkoff.prepare")]
    [InlineData("BatchDetails", typeof(CheckOffController), nameof(CheckOffController.GetBatchDetails), "checkoff.view", "checkoff.post")]
    [InlineData("Payroll", typeof(PayrollController), nameof(PayrollController.GetDashboardStats), "hr.payroll.view", "hr.payroll.run")]
    [InlineData("AccountingDlq", typeof(FinanceController), nameof(FinanceController.GetIntegrationErrors), "accounting-integrations.view", "accounting-integrations.retry")]
    [InlineData("GlMappings", typeof(FinanceController), nameof(FinanceController.GetGlMappings), "gl-mappings.view", "gl-mappings.manage")]
    [InlineData("LoanGuarantees", typeof(LoanApplicationsController), nameof(LoanApplicationsController.GetGuarantorSummary), "applications.view", "applications.approve")]
    [InlineData("LoanApplications", typeof(LoanApplicationsController), nameof(LoanApplicationsController.GetApplications), "applications.view", "applications.approve")]
    public async Task Signed_in_route_and_api_require_the_same_read_grant(
        string component, Type controller, string action, string readPermission, string writePermission)
    {
        var route = typeof(SaccoOperationPolicies).Assembly.GetTypes().Single(type => type.Name == component);
        var routePolicy = route.GetCustomAttribute<AuthorizeAttribute>()!.Policy!;
        var endpointPolicy = controller.GetMethod(action)!.GetCustomAttribute<RequirePermissionAttribute>()!.Policy!;
        Assert.Equal(readPermission, routePolicy);
        Assert.Equal(PermissionAuthorizationConstants.BuildPolicyName(readPermission), endpointPolicy);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(SaccoOperationPolicies.Configure);
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        var reader = Principal(new Claim("permission", readPermission));
        var operatorWithoutRead = Principal(new Claim("permission", writePermission));
        var administrator = Principal(new Claim(ClaimTypes.Role, "System Administrator"));

        foreach (var policy in new[] { routePolicy, endpointPolicy })
        {
            Assert.False((await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), policy)).Succeeded);
            Assert.False((await authorization.AuthorizeAsync(operatorWithoutRead, policy)).Succeeded);
            Assert.True((await authorization.AuthorizeAsync(reader, policy)).Succeeded);
            Assert.True((await authorization.AuthorizeAsync(administrator, policy)).Succeeded);
        }

        Assert.False((await authorization.AuthorizeAsync(reader, writePermission)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(operatorWithoutRead, writePermission)).Succeeded);
    }

    private static ClaimsPrincipal Principal(Claim claim) =>
        new(new ClaimsIdentity([claim], "SaccoOperationTest"));
}
