using LS.Application.Features.IAM.Menus;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.SharedKernel.Features.IAM.Menus;

namespace LS.Tests.Unit.Features.IAM;

public sealed class NavigationRulesTests
{
    [Theory]
    [InlineData("/admin/menus", "users.view", false)]
    [InlineData("/admin/access-catalog", "roles.view", false)]
    [InlineData("/admin/menus", "menus.view", true)]
    [InlineData("/admin/iam", "users.view", true)]
    [InlineData("/admin/iam", "roles.view", true)]
    [InlineData("/admin/iam", "roles.manage_permissions", true)]
    [InlineData("/admin/iam", "users.edit", false)]
    [InlineData("/admin/employees", "employees.view", true)]
    [InlineData("/admin/employees", "departments.view", false)]
    [InlineData("/admin/departments", "departments.view", true)]
    [InlineData("/admin", "employees.view", true)]
    [InlineData("/admin", "control_plane.view_tenants", false)]
    [InlineData("/Admin/Menus/?tab=1", "menus.view", true)]
    [InlineData("/loans/applications", "anything", true)]
    public void Shipped_administration_pages_require_a_read_operation(string path, string permission, bool allowed)
    {
        Assert.Equal(allowed, AdministrationPageAccess.CanOpen(path, [permission]));
    }

    [Fact]
    public void A_child_is_hidden_when_any_ancestor_is_not_permitted()
    {
        var root = Menu("root", null);
        var middle = Menu("middle", root.Id);
        var leaf = Menu("leaf", middle.Id);
        var sibling = Menu("sibling", root.Id);

        var visible = NavigationHierarchy.Visible([root, middle, leaf, sibling], menu => menu.Key != "middle");

        Assert.Equal(["root", "sibling"], visible.Select(static menu => menu.Key).Order().ToArray());
    }

    [Fact]
    public void Orphans_and_cycles_are_not_visible()
    {
        var orphan = Menu("orphan", Guid.CreateVersion7());
        var first = Menu("first", null);
        var second = Menu("second", first.Id);
        first.Update(second.Id, null, "first", "first", "", "/first", "icon", "Sidebar", null, null, 1, true, "test");

        var visible = NavigationHierarchy.Visible([orphan, first, second], static _ => true);

        Assert.Empty(visible);
    }

    private static MenuItem Menu(string key, Guid? parentId)
        => MenuItem.Create(parentId, null, key, key, "", "/" + key, "icon", "Sidebar", null, null, 1, "test");
}
