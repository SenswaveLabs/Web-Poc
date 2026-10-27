using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using Senswave.Web.Devices.Components;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Devices.Models;
using Senswave.Web.Devices.Services;
using Senswave.Web.Homes.Integration;
using Senswave.Web.Homes.Services;
using Senswave.Web.Pages.Devices;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Devices;

public class DeviceDetailsPageTests : ComponentTestBase
{
    private readonly IDeviceDetailsService _details = Substitute.For<IDeviceDetailsService>();
    private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

    // The second load of the device, after the change, waits until the test completes it.
    private readonly TaskCompletionSource<Result<DeviceModel>> _reload = new();

    public DeviceDetailsPageTests()
    {
        var operations = Substitute.For<IOperationIntegrationService>();
        operations.DisplayOperationsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(new DisplayOperationsResponse([]));

        var rooms = Substitute.For<IRoomService>();
        rooms.Rooms.Returns(new List<RoomDto>());
        rooms.LoadRooms().Returns(Result.Success());

        var homes = Substitute.For<IHomeService>();
        homes.Initialize().Returns(Result.Success());

        _details.GetDevice("device-1").Returns(
            Task.FromResult(Result<DeviceModel>.Success(new DeviceModel { Id = "device-1", Name = "Lamp" })),
            _reload.Task);
        _details.GetWidgetsAndOperations("device-1").Returns(Result<List<DisplayGroupDto>>.Success([]));
        _details.GetDashboards("device-1").Returns(Result<List<DetailedDashboardDto>>.Success([]));

        // A dialog that closes with OK, as if the user created an operation.
        var reference = Substitute.For<IDialogReference>();
        reference.Result.Returns(Task.FromResult<DialogResult?>(DialogResult.Ok(true)));
        _dialogs.ShowAsync<AddOperationDialog>(Arg.Any<string>(), Arg.Any<DialogParameters>(), Arg.Any<DialogOptions>())
            .Returns(reference);

        Services.AddLogging();
        Services.AddSingleton(operations);
        Services.AddSingleton(Substitute.For<IWidgetIntegrationService>());
        Services.AddSingleton(Substitute.For<IDashboardIntegrationService>());
        Services.AddSingleton(_details);
        Services.AddSingleton(rooms);
        Services.AddSingleton(homes);
        Services.AddSingleton(Substitute.For<IErrorFactory>());
        Services.AddSingleton(_dialogs);
    }

    private static string ActiveTab(IRenderedComponent<Details> cut) => cut.Find(".mud-tab.mud-tab-active").TextContent.Trim();

    [Fact]
    public void Adding_an_operation_keeps_the_operations_tab_open()
    {
        var cut = Render<Details>(p => p.Add(x => x.Id, "device-1"));
        cut.WaitForAssertion(() => Assert.Equal("General", ActiveTab(cut)));

        cut.FindAll(".mud-tab").First(t => t.TextContent.Contains("Operations")).Click();
        Assert.StartsWith("Operations", ActiveTab(cut));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Operation").Click();

        // While the device reloads, the tabs stay on screen instead of the loading skeleton.
        cut.WaitForAssertion(() => _details.Received(2).GetDevice("device-1"));
        Assert.StartsWith("Operations", ActiveTab(cut));

        _reload.SetResult(Result<DeviceModel>.Success(new DeviceModel { Id = "device-1", Name = "Lamp" }));
        cut.WaitForAssertion(() => Assert.StartsWith("Operations", ActiveTab(cut)));
    }
}
