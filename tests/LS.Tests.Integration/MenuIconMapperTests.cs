extern alias Blazor;

using Blazor::LS.UI.Blazor.Components.Common;
using MudBlazor;
using Xunit;

namespace LS.Tests.Integration;

/// <summary>
/// Menu icons are stored by name (e.g. "EditNote") and resolved against MudBlazor's Material icons. The resolver
/// must find any Material icon, not only the curated options, and must survive MudBlazor declaring names that differ
/// only by case (AddChart / Addchart), which once crashed every component that rendered an icon.
/// </summary>
public sealed class MenuIconMapperTests
{
    [Theory]
    [InlineData("EditNote")]
    [InlineData("Receipt")]
    [InlineData("TrendingUp")]
    [InlineData("AccountBalanceWallet")]
    [InlineData("SyncProblem")]
    [InlineData("Payments")]
    [InlineData("addchart")]
    public void Resolves_any_material_icon_by_name(string name)
    {
        var icon = MenuIconMapper.Resolve(name);

        Assert.False(string.IsNullOrWhiteSpace(icon));
        Assert.NotEqual(Icons.Material.Filled.Menu, icon);
    }

    [Fact]
    public void Resolves_exact_icons_and_falls_back_for_unknown_names()
    {
        Assert.Equal(Icons.Material.Filled.EditNote, MenuIconMapper.Resolve("editnote"));
        Assert.Equal(Icons.Material.Filled.Menu, MenuIconMapper.Resolve("not-an-icon"));
        Assert.Equal(Icons.Material.Filled.Menu, MenuIconMapper.Resolve(null));
    }
}
