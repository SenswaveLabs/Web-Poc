using System.Text.Json.Nodes;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Senswave.Web.Devices.Components;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Devices;

public class SensDashboardTests : ComponentTestBase
{
    public SensDashboardTests()
    {
        Services.AddSingleton(Substitute.For<IWidgetIntegrationService>());
        Services.AddSingleton(Substitute.For<IErrorFactory>());
    }

    private static CalculatedWidgetDto Button(string id, string name) =>
        new(id, name, "Button", new WidgetDetailsDto(null, null, null, null, new WidgetRuntimeDto(JsonValue.Create(""))), DateTime.UtcNow);

    // Widgets in rows 0 and 3 of a 6 x 4 grid; rows 1, 2, 4 and 5 are empty.
    private IRenderedComponent<SensDashboard> RenderGrid(bool editMode) => Render<SensDashboard>(p => p
        .Add(x => x.Id, "dash-1")
        .Add(x => x.Rows, 6)
        .Add(x => x.Columns, 4)
        .Add(x => x.EditMode, editMode)
        .Add(x => x.PositionedWidgets,
        [
            new PositionedWidgetDto("w1", 0, 0, 1, 2),
            new PositionedWidgetDto("w2", 3, 0, 1, 4)
        ])
        .Add(x => x.CalculatedWidgets, [Button("w1", "Full"), Button("w2", "Low")])
        .Add(x => x.OnUnlinkWidget, (_, _) => Task.CompletedTask));

    private static string GridStyle(IRenderedComponent<SensDashboard> cut) =>
        cut.FindAll("div[style]").First(e => e.GetAttribute("style")!.Contains("display: grid")).GetAttribute("style")!;

    [Fact]
    public void View_mode_skips_empty_rows()
    {
        var cut = RenderGrid(editMode: false);

        Assert.Contains("grid-template-rows: repeat(2,", GridStyle(cut));

        var widgetStyles = cut.FindAll("div.pa-1").Select(e => e.GetAttribute("style")).ToList();
        Assert.Contains(widgetStyles, s => s!.Contains("grid-row: 1 / span 1"));
        Assert.Contains(widgetStyles, s => s!.Contains("grid-row: 2 / span 1"));
    }

    [Fact]
    public void Edit_mode_keeps_full_grid_with_empty_cells()
    {
        var cut = RenderGrid(editMode: true);

        Assert.Contains("grid-template-rows: repeat(6, 1fr)", GridStyle(cut));
        // 24 cells minus 2 + 4 occupied cells.
        Assert.Equal(18, cut.FindAll("button.border-dashed").Count);
    }

    [Fact]
    public void Empty_screen_explains_how_to_add_widgets()
    {
        var cut = Render<SensDashboard>(p => p
            .Add(x => x.PositionedWidgets, [])
            .Add(x => x.CalculatedWidgets, [])
            .Add(x => x.OnUnlinkWidget, (_, _) => Task.CompletedTask));

        Assert.Contains("No controls on this screen yet", cut.Markup);
    }
}
