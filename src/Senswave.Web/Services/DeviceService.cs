using Refit;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Devices.Models;
using Senswave.Web.Devices.Services;
using Senswave.Web.Homes.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Services;

public class DeviceService(
    ILogger<DeviceService> logger, 
    IDeviceIntegrationService integrationService,
    IDashboardIntegrationService dashboardIntegrationService,
    IWidgetIntegrationService widgetIntegrationService,
    IHomeService homeService, 
    IRoomService roomService,
    IErrorFactory errorFactory) : IDeviceListService, IDeviceDetailsService
{
    public async Task<Result> CreateDevice(DeviceModel dto)
    {
        try
        {
            var homeId = homeService.CurrentHome?.Id ?? string.Empty;

            var request = new CreateDeviceRequest(homeId, dto.RoomId, dto.Name, dto.Icon);

            var devices = await integrationService.CreateDeviceAsync(request);

            logger.LogInformation("Returnigng devices for home");
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to create devices.");
            return await errorFactory.FromApiExceptionAsync(ex, "FailedToCreateDevice");
        }
    }

    public async Task<Result<List<DetailedDashboardDto>>> GetDashboards(string id)
    {
        try
        {
            var homeId = homeService.CurrentHome?.Id ?? string.Empty;

            var devices = await dashboardIntegrationService.DisplayDashboardsAsync(id);

            var dashboardList = new List<DetailedDashboardDto>();

            foreach (var dashboard in devices.Items)
            {
                var details = await dashboardIntegrationService.GetDashboardDisplayAsync(dashboard.Id);

                dashboardList.Add(new DetailedDashboardDto(
                    dashboard.Id,
                    dashboard.Name,
                    dashboard.Icon,
                    dashboard.Type,
                    details.Configuration["rows"].AsValue().GetValue<int>(),
                    details.Configuration["columns"].AsValue().GetValue<int>()));
            }

            logger.LogInformation("Returnigng dashboard for home");
            return Result<List<DetailedDashboardDto>>.Success(dashboardList);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("No dashboards in home");
            return Result<List<DetailedDashboardDto>>.Success([]);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get dashboards.");
            return await errorFactory.FromApiExceptionAsync<List<DetailedDashboardDto>>(ex, "FailedToGetDashboards");
        }
    }

    public async Task<Result<List<DisplayGroupDto>>> GetWidgetsAndOperations(string id)
    {
        try
        {
            var homeId = homeService.CurrentHome?.Id ?? string.Empty;

            var devices = await widgetIntegrationService.DisplayWidgetsAsync(id);

            logger.LogInformation("Returnigng devices for home");
            return Result<List<DisplayGroupDto>>.Success(devices.Items);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("No devices in home");
            return Result<List<DisplayGroupDto>>.Success([]);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get devices.");
            return await errorFactory.FromApiExceptionAsync<List<DisplayGroupDto>>(ex, "FailedToGetWigetGroups");
        }
    }

    public async Task<Result<DeviceModel>> GetDevice(string id)
    {
        try
        {
            var homeId = homeService.CurrentHome?.Id ?? string.Empty;

            var device = await integrationService.GetDeviceAsync(id);

            var model = new DeviceModel
            {
                Id = id,
                HomeId = homeId,
                Icon = device.Icon,
                RoomId = device.RoomId,
                Name = device.Name,
                TileOperationId = device.Tile?.OperationId,
                TileType = device.Tile?.Type,
            };

            logger.LogInformation("Returnigng devices for home");
            return Result<DeviceModel>.Success(model);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get device.");
            return await errorFactory.FromApiExceptionAsync<DeviceModel>(ex, "FailedToGetDevice");
        }
    }

    public async Task<Result<List<DisplayDeviceDto>>> GetListDevicesForHome()
    {
        try
        {
            var homeId = homeService.CurrentHome?.Id ?? string.Empty;

            var devices = await integrationService.DisplayDevicesAsync(homeId, 1,100);

            logger.LogInformation("Returnigng devices for home");
            return Result<List<DisplayDeviceDto>>.Success(devices.Items);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("No devices in home");
            return Result<List<DisplayDeviceDto>>.Success([]);
        }
        catch (ApiException ex) 
        {
            logger.LogError(ex, "Failed to get devices.");
            return await errorFactory.FromApiExceptionAsync<List<DisplayDeviceDto>>(ex, "FailedToGetDevices");
        }
    }

    public Task<string> GetRoomNameById(string id)
    {
        var room = roomService.Rooms.Where(x => x.Id == id).FirstOrDefault();

        if (room is null)
            return Task.FromResult(string.Empty);

        return Task.FromResult(room.Name);
    }

    public Task<Result> UpdateDevice(DeviceModel dto)
    {
        throw new NotImplementedException();
    }
}
