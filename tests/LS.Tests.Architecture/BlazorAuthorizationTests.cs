using System.Reflection;
using LS.UI.Blazor.Components.Layout;
using LS.UI.Blazor.Components.Pages.Admin;
using LS.UI.Blazor.Features.IAM.Users.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace LS.Tests.Architecture;

public sealed class BlazorAuthorizationTests
{
    [Fact]
    public void Organization_Settings_Page_Retains_Its_Route_And_Authorization()
    {
        var page = typeof(AdminOrgSettings);
        var route = Assert.Single(page.GetCustomAttributes<RouteAttribute>());

        Assert.Equal("/admin/org-settings", route.Template);
        Assert.Single(page.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Null(page.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    // ControlPlaneLayout carries the platform-only ControlPlane.Access policy (platform permission + MFA).
    // Only control-panel routes may use it; a tenant (SACCO) page using it locks tenant staff out.
    [Fact]
    public void Only_Control_Panel_Routes_Use_The_Platform_Layout_Or_Policy()
    {
        var offenders = typeof(MainLayout).Assembly.GetTypes()
            .Where(type => typeof(IComponent).IsAssignableFrom(type))
            .Select(type => (Type: type, Routes: type.GetCustomAttributes<RouteAttribute>().Select(route => route.Template).ToList()))
            .Where(page => page.Routes.Count > 0 && !page.Routes.All(IsControlPanelRoute))
            .Where(page =>
                page.Type.GetCustomAttribute<LayoutAttribute>()?.LayoutType == typeof(ControlPlaneLayout)
                || page.Type.GetCustomAttributes<AuthorizeAttribute>().Any(a => a.Policy == "ControlPlane.Access"))
            .Select(page => $"{page.Type.FullName} ({string.Join(", ", page.Routes)})")
            .ToList();

        Assert.True(offenders.Count == 0, "Tenant pages must not use ControlPlaneLayout or ControlPlane.Access: " + string.Join("; ", offenders));
    }

    [Theory]
    [InlineData(typeof(LS.UI.Blazor.Features.Banking.Pages.Banking), "banking-products.view")]
    [InlineData(typeof(LS.UI.Blazor.Features.Banking.Pages.SavingsProducts), "banking-products.view")]
    [InlineData(typeof(LS.UI.Blazor.Features.Banking.Pages.ShareProducts), "banking-products.view")]
    [InlineData(typeof(LS.UI.Blazor.Features.Loans.Pages.LoanProducts), "products.view")]
    [InlineData(typeof(LS.UI.Blazor.Features.Loans.Pages.ApplyForLoan), "applications.create")]
    public void Sacco_Product_Pages_Use_Their_Tenant_Permission_Policy(Type page, string policy)
    {
        Assert.Equal(policy, Assert.Single(page.GetCustomAttributes<AuthorizeAttribute>()).Policy);
        Assert.Null(page.GetCustomAttribute<LayoutAttribute>());
    }

    private static bool IsControlPanelRoute(string template)
        => template == "/control-panel" || template.StartsWith("/control-panel/", StringComparison.Ordinal);

    // Signed-out users must be able to stay on these pages: they may not require authorization
    // and must render in the anonymous auth layout, never the authenticated MainLayout.
    [Theory]
    [InlineData(typeof(SignIn), "/iam/sign-in")]
    [InlineData(typeof(ForgotPassword), "/iam/forgot-password")]
    [InlineData(typeof(ResetPassword), "/iam/reset-password")]
    public void Anonymous_Account_Pages_Do_Not_Require_Authorization(Type page, string template)
    {
        Assert.Contains(page.GetCustomAttributes<RouteAttribute>(), route => route.Template == template);
        Assert.Empty(page.GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal(typeof(AuthLayout), page.GetCustomAttribute<LayoutAttribute>()?.LayoutType);
    }
}
