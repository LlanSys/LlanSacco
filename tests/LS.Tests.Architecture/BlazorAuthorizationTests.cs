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
