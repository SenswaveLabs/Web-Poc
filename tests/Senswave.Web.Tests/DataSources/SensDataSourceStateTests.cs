using Bunit;
using Microsoft.AspNetCore.Components;
using Senswave.Web.DataSources.Components;

namespace Senswave.Web.Tests.DataSources;

public class SensDataSourceStateTests : ComponentTestBase
{
    [Theory]
    [InlineData("Working", "Connected", "Connection working correctly")]
    [InlineData("Stopped", "Stopped", "Broker has been stopped")]
    [InlineData("NotStarted", "Not started", "Broker has not been started")]
    [InlineData("Restarting", "Restarting", "Broker is restarting")]
    [InlineData("Something else", "Unknown", "Failed to determine broker state")]
    public void Shows_status_message_once(string status, string message, string description)
    {
        var cut = Render<SensDataSourceState>(p => p.Add(x => x.ConnectionStatus, status));

        var region = cut.Find("[role=status]");
        Assert.Contains(message, region.TextContent);
        Assert.Contains(description, region.TextContent);
        // The status is no longer repeated in a separate chip.
        Assert.Empty(cut.FindAll(".mud-chip"));
    }

    [Fact]
    public void Shows_broker_name_and_address()
    {
        var cut = Render<SensDataSourceState>(p => p
            .Add(x => x.Name, "Test Broker")
            .Add(x => x.Server, "broker.example.com")
            .Add(x => x.Port, 8883));

        Assert.Contains("Test Broker", cut.Markup);
        Assert.Contains("broker.example.com:8883", cut.Markup);
    }

    [Fact]
    public void Renders_actions_inside_status()
    {
        RenderFragment actions = builder => builder.AddMarkupContent(0, "<button id=\"restart\">Restart</button>");

        var cut = Render<SensDataSourceState>(p => p
            .Add(x => x.ConnectionStatus, "Working")
            .Add(x => x.Actions, actions));

        Assert.NotNull(cut.Find("[role=status] #restart"));
    }

    [Fact]
    public void Actions_are_hidden_on_print()
    {
        RenderFragment actions = builder => builder.AddMarkupContent(0, "<button id=\"restart\">Restart</button>");

        var cut = Render<SensDataSourceState>(p => p
            .Add(x => x.ConnectionStatus, "Working")
            .Add(x => x.Actions, actions));

        Assert.NotNull(cut.Find(".no-print #restart"));
    }

    [Fact]
    public void Home_mode_shows_section_title()
    {
        var cut = Render<SensDataSourceState>(p => p.Add(x => x.HomeMode, true));

        Assert.Contains("Data source", cut.Markup);
    }
}
