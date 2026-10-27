using System.Text.Json.Nodes;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Senswave.Web.Devices.Components;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Devices;

public class SensWidgetTests : ComponentTestBase
{
    private readonly IWidgetIntegrationService _widgets = Substitute.For<IWidgetIntegrationService>();

    public SensWidgetTests()
    {
        Services.AddSingleton(_widgets);
        Services.AddSingleton(Substitute.For<IErrorFactory>());
    }

    private IRenderedComponent<SensWidget> RenderWidget(string type, JsonNode? value, WidgetRangeDto? range = null, string? unit = null, bool editMode = false) =>
        Render<SensWidget>(p => p
            .Add(x => x.Widget, new CalculatedWidgetDto("w1", "Brightness", type,
                new WidgetDetailsDto(unit, 0.01, range, null, new WidgetRuntimeDto(value)), DateTime.UtcNow))
            .Add(x => x.Position, new PositionedWidgetDto("w1", 0, 0, 1, 4))
            .Add(x => x.EditMode, editMode));

    [Theory]
    [InlineData(0.35, "35%")]
    [InlineData(1.0, "100%")]
    public void Slider_with_unit_range_shows_percent(double value, string expected)
    {
        var cut = RenderWidget("Slider", JsonValue.Create(value), new WidgetRangeDto(0, 1));

        Assert.Equal(expected, cut.Find(".mud-typography-h6").TextContent.Trim());
    }

    [Fact]
    public void Slider_with_other_range_shows_value_and_unit_with_invariant_decimal()
    {
        var cut = RenderWidget("Slider", JsonValue.Create(21.5), new WidgetRangeDto(10, 30), "°C");

        Assert.Equal("21.5°C", cut.Find(".mud-typography-h6").TextContent.Trim());
    }

    [Theory]
    [InlineData(true, "On")]
    [InlineData(false, "Off")]
    public void Switch_shows_state_label(bool state, string label)
    {
        var cut = RenderWidget("Switch", JsonValue.Create(state.ToString().ToLowerInvariant()));

        Assert.Contains(label, cut.FindAll(".mud-typography-caption").Select(e => e.TextContent));
    }

    [Fact]
    public void Switch_click_sends_toggled_value()
    {
        var cut = RenderWidget("Switch", JsonValue.Create("false"));

        cut.Find("button").Click();

        _widgets.Received(1).ExecuteWidgetActionAsync("w1", Arg.Is<WidgetActionRequest>(r => r.Value.GetValue<bool>()));
    }

    [Fact]
    public void Edit_mode_shows_remove_button_with_accessible_name()
    {
        var cut = RenderWidget("Button", JsonValue.Create(""), editMode: true);

        Assert.NotNull(cut.Find("button[aria-label='Remove Brightness from screen']"));
    }

    [Fact]
    public void Unknown_type_is_reported()
    {
        var cut = RenderWidget("Gauge", null);

        Assert.Contains("Invalid: Gauge", cut.Markup);
    }

    [Fact]
    public void Button_widget_is_hidden_on_print()
    {
        var cut = RenderWidget("Button", JsonValue.Create(""));

        Assert.Contains("no-print", cut.Find("div").ClassList);
    }

    [Fact]
    public void Button_widget_in_edit_mode_stays_on_print()
    {
        var cut = RenderWidget("Button", JsonValue.Create(""), editMode: true);

        Assert.DoesNotContain("no-print", cut.Find("div").ClassList);
    }

    [Fact]
    public void Slider_on_print_hides_the_slider_and_keeps_the_value()
    {
        var cut = RenderWidget("Slider", JsonValue.Create(21.5), new WidgetRangeDto(10, 30), "°C");

        Assert.Contains("no-print", cut.Find(".mud-slider").ClassList);
        Assert.DoesNotContain("no-print", cut.Find(".mud-typography-h6").ClassList);
    }

    [Fact]
    public void Radio_on_print_keeps_only_the_selected_option()
    {
        var options = new List<RadioOption>
        {
            new("auto", "Auto", ""),
            new("eco", "Eco", ""),
            new("off", "Off", ""),
        };
        var cut = Render<SensWidget>(p => p
            .Add(x => x.Widget, new CalculatedWidgetDto("w1", "Mode", "Radio",
                new WidgetDetailsDto(null, null, null, options, new WidgetRuntimeDto(JsonValue.Create("eco"))), DateTime.UtcNow))
            .Add(x => x.Position, new PositionedWidgetDto("w1", 0, 0, 1, 4)));

        var printed = cut.FindAll("button").Where(b => !b.ClassList.Contains("no-print")).Select(b => b.TextContent.Trim());
        Assert.Equal(["Eco"], printed);
    }
}
