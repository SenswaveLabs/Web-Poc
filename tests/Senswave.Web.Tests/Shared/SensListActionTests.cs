using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Senswave.Web.Shared.Components;
using Senswave.Web.Shared.Components.Enums;

namespace Senswave.Web.Tests.Shared;

public class SensListActionTests : ComponentTestBase
{
    [Theory]
    [InlineData(SensListActionType.Add, "Add")]
    [InlineData(SensListActionType.Edit, "Edit")]
    [InlineData(SensListActionType.Join, "Join")]
    public void Uses_default_title_for_type(SensListActionType type, string title)
    {
        var cut = Render<SensListAction>(p => p.Add(x => x.Type, type));

        Assert.Equal(title, cut.Find(".mud-typography-subtitle1").TextContent);
    }

    [Fact]
    public void Shows_title_and_description()
    {
        var cut = Render<SensListAction>(p => p
            .Add(x => x.Title, "Add device")
            .Add(x => x.Description, "Create a new device"));

        Assert.Contains("Add device", cut.Markup);
        Assert.Contains("Create a new device", cut.Markup);
    }

    [Fact]
    public void Is_a_keyboard_reachable_button()
    {
        var cut = Render<SensListAction>();

        var root = cut.Find(".sens-list-action");
        Assert.Equal("button", root.GetAttribute("role"));
        Assert.Equal("0", root.GetAttribute("tabindex"));
    }

    [Fact]
    public void Click_invokes_callback()
    {
        var clicks = 0;
        var cut = Render<SensListAction>(p => p.Add(x => x.OnClick, () => clicks++));

        cut.Find(".sens-list-action").Click();

        Assert.Equal(1, clicks);
    }

    [Theory]
    [InlineData("Enter", 1)]
    [InlineData(" ", 1)]
    [InlineData("Tab", 0)]
    public void Enter_and_space_invoke_callback(string key, int expected)
    {
        var clicks = 0;
        var cut = Render<SensListAction>(p => p.Add(x => x.OnClick, () => clicks++));

        cut.Find(".sens-list-action").KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, clicks);
    }
}
