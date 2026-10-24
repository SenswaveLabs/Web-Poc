using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Senswave.Web.Devices.Components;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Devices.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Devices;

public class SensDeviceTests : ComponentTestBase
{
    private readonly IDeviceIntegrationService _integration = Substitute.For<IDeviceIntegrationService>();
    private readonly IDeviceDetailsService _details = Substitute.For<IDeviceDetailsService>();

    public SensDeviceTests()
    {
        Services.AddSingleton(_integration);
        Services.AddSingleton(_details);
        Services.AddSingleton(Substitute.For<IErrorFactory>());
    }

    private static DisplayDeviceDto Device(string? tileType = null, JsonNode? value = null, string roomId = "") => new()
    {
        Id = "device-1",
        Name = "Pico Controller",
        Icon = "radio-outline",
        RoomId = roomId,
        Tile = tileType is null ? null : new DisplayDeviceTileDto { Type = tileType, Value = value }
    };

    [Fact]
    public void Shows_room_name_when_device_is_in_a_room()
    {
        _details.GetRoomNameById("room-1").Returns("Kitchen");

        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device(roomId: "room-1")));

        cut.WaitForAssertion(() => Assert.Contains("Kitchen", cut.Markup));
    }

    [Fact]
    public void Shows_no_room_instead_of_unknown()
    {
        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device()));

        Assert.Contains("No room", cut.Markup);
        Assert.DoesNotContain("Unknown", cut.Markup);
    }

    [Theory]
    [InlineData(true, "On")]
    [InlineData(false, "Off")]
    public void Switch_tile_shows_state_label(bool state, string label)
    {
        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device("Switch", JsonValue.Create(state))));

        Assert.Equal(label, cut.Find("b").TextContent);
        Assert.Equal(state, cut.Find(".sens-device").ClassList.Contains("sens-device--on"));
    }

    [Fact]
    public void Switch_tile_without_value_shows_no_data()
    {
        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device("Switch")));

        Assert.Equal("No data", cut.Find("b").TextContent);
    }

    [Fact]
    public void Default_tile_offers_to_open_controls()
    {
        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device("Default")));

        Assert.Contains("Open controls", cut.Markup);
        Assert.Empty(cut.FindAll("input[type=checkbox]"));
    }

    [Fact]
    public async Task Toggling_switch_sends_new_state_and_does_not_open_device()
    {
        var opened = false;
        var device = Device("Switch", JsonValue.Create(false));

        var cut = Render<SensDevice>(p => p
            .Add(x => x.Device, device)
            .Add(x => x.OnClick, _ => opened = true));

        await cut.Find("input[type=checkbox]").ChangeAsync(new ChangeEventArgs { Value = true });

        await _integration.Received(1).ExecuteDeviceTileActionAsync(
            "device-1",
            Arg.Is<DeviceTileActionRequest>(r => r.Value.GetValue<bool>()));
        Assert.True(device.Tile!.Value!.GetValue<bool>());
        Assert.False(opened);
    }

    [Fact]
    public void Clicking_tile_opens_device()
    {
        DisplayDeviceDto? opened = null;
        var device = Device("Default");

        var cut = Render<SensDevice>(p => p
            .Add(x => x.Device, device)
            .Add(x => x.OnClick, d => opened = d));

        cut.Find(".sens-device").Click();

        Assert.Same(device, opened);
    }

    [Fact]
    public void Configure_menu_item_navigates_to_device_details()
    {
        var cut = Render<SensDevice>(p => p.Add(x => x.Device, Device("Default")));

        cut.Find("button[aria-label='Pico Controller options']").Click();
        Popovers.WaitForElement(".mud-menu-item").Click();

        // MudMenuItem closes the menu before it runs the item action.
        var navigation = Services.GetRequiredService<NavigationManager>();
        cut.WaitForAssertion(() => Assert.EndsWith("/devices/details/device-1", navigation.Uri));
    }
}
