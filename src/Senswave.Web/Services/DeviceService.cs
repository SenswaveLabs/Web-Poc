using Refit;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Devices.Services;
using Senswave.Web.Homes.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Services;

public class DeviceService(
    ILogger<DeviceService> logger, 
    IDeviceIntegrationService integrationService,
    IHomeService homeService, 
    IRoomService roomService,
    IErrorFactory errorFactory) : IDeviceListService, IDeviceDetailsService
{
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
            return Task.FromResult("");

        return Task.FromResult(room.Name);
    }
}
