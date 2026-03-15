using Refit;
using Senswave.Web.Homes.Services;
using Senswave.Web.Integration.Homes;
using Senswave.Web.Shared.Resulting;
using System.Net;

namespace Senswave.Web.Services;

public class RoomService(ILogger<RoomService> logger, 
    IErrorFactory errorFactory, 
    IRoomsIntegrationServcice roomsIntegrationService, 
    IHomeService homeService) : IRoomService
{
    public async Task<Result<List<RoomDto>>> GetRooms()
    {
        try
        {
            var homeId = homeService.CurrentHome!.Id;

            var roomsResponse = await roomsIntegrationService.GetRoomsAsync(homeId);

            return Result<List<RoomDto>>.Success(roomsResponse.Items);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("No rooms in home");
            return Result<List<RoomDto>>.Success([]);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to load rooms");
            return await errorFactory.FromApiExceptionAsync<List<RoomDto>>(ex, "FailedToLoadRooms");
        }
    }
}
