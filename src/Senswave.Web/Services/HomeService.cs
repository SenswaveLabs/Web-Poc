using Refit;
using Senswave.Web.Homes.Integration;
using Senswave.Web.Homes.Models;
using Senswave.Web.Homes.Services;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Themes;
using System.Net;
using System.Xml.Linq;

namespace Senswave.Web.Services;

public class HomeService(
    IErrorFactory errorFactory,
    IRoomsIntegrationServcice roomsIntegrationService,
    IHomesIntegrationService integrationService,
    ILogger<HomeService> logger) : IHomeService, IRoomService
{
    private HomeDetails? _currentHome;

    private List<RoomDto> _rooms = [];

    private readonly SemaphoreSlim _initLock = new(1, 1);

    public HomeDetails? CurrentHome
    {
        set
        {
            _currentHome = value;
            OnChange?.Invoke();
        }
        get => _currentHome;
    }

    public List<RoomDto> Rooms => _rooms;

    public event Action? OnChange;

    public async Task<Result> Initialize()
    {
        await _initLock.WaitAsync();

        try
        {
            if (_currentHome is not null)
            {
                logger.LogInformation("[Home: {homeId}] Skipping home initialization.", _currentHome.Id);
                return Result.Success();
            }

            logger.LogInformation("No current home - initializing.");

            string currentHomeId = string.Empty;

            try
            {
                var currentHomeResponse = await integrationService.GetCurrentHome();
                currentHomeId = currentHomeResponse.Id;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, "Failed to get current home for user");
                CurrentHome = null;
                return await errorFactory.FromApiExceptionAsync(ex, "GetCurrentHomeFailedUnexpectedly");
            }

            try
            {
                var response = await integrationService.GetHome(currentHomeId);

                if (response is null)
                {
                    logger.LogWarning("[Home: {homeId}] Failed to get home. No response from integration service", currentHomeId);
                    return errorFactory.Create<HomeDetails>("GetHomeFailedUnexpectedly");
                }

                CurrentHome = new HomeDetails
                {
                    Id = response.Id,
                    Name = response.Name,
                    Icon = response.Icon,
                    IsOwner = response.IsOwner,
                    DataSource = response.DataSource is null ? null : new DataSource
                    {
                        Id = response.DataSource?.Id ?? string.Empty,
                        Name = response.DataSource?.Name ?? string.Empty,
                        State = response.DataSource?.State ?? string.Empty
                    },
                    Location = response.Location is null ? null : new Location
                    {
                        Longitude = response.Location.Longitude,
                        Latitude = response.Location.Latitude
                    },
                    Rooms = response.Rooms?.Select(r => new Room
                    {
                        Id = r.Id,
                        Name = r.Name
                    }).ToList() ?? []
                };

                return Result.Success();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, "[Home: {homeId}] Failed to get homes for user.", currentHomeId);
                _currentHome = null;
                return await errorFactory.FromApiExceptionAsync(ex, "FailedToLoadCurrentHomePleaseRefresh");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during home service initialization");
            CurrentHome = null;
            return errorFactory.Create("HomeServiceInitializationFailedUnexpectedly", "An unexpected error occurred during home service initialization.");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<Result<List<Home>>> GetHomes()
    {
        logger.LogInformation("Getting homes for user");

        try
        {
            var response = await integrationService.GetHomes();

            var finalItems = response.Items.Select(x => new Home
            {
                Id = x.Id,

                DataSourceId = x.DataSourceId,

                Name = x.Name,
                Icon = x.Icon,

                IsOwner = x.IsOwner
            }).ToList();

            return Result<List<Home>>.Success(finalItems);
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<List<Home>>.Success([]);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get homes for user");
            return await errorFactory.FromApiExceptionAsync<List<Home>>(ex, "GetHomesFailed");
        }
    }
    
    public async Task<Result> ChangeHome(string newHomeId)
    {
        await _initLock.WaitAsync();

        try
        {
            logger.LogInformation("[Home: {homeId}] Switching home.", newHomeId);

            var response = await integrationService.GetHome(newHomeId);

            if (response is null)
            {
                logger.LogWarning("[Home: {homeId}] Home change failed. No response from integration service", newHomeId);
                return errorFactory.Create<HomeDetails>("GetHomeFailedUnexpectedly");
            }

            CurrentHome = new HomeDetails
            {
                Id = response.Id,
                Name = response.Name,
                Icon = response.Icon,
                IsOwner = response.IsOwner,
                DataSource = response.DataSource is null ? null : new DataSource
                {
                    Id = response.DataSource?.Id ?? string.Empty,
                    Name = response.DataSource?.Name ?? string.Empty,
                    State = response.DataSource?.State ?? string.Empty
                },
                Location = response.Location is null ? null : new Location
                {
                    Longitude = response.Location.Longitude,
                    Latitude = response.Location.Latitude
                },
                Rooms = response.Rooms?.Select(r => new Room
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList() ?? []
            };

            logger.LogInformation("[Home: {homeId}] Home changed.", newHomeId);

            return Result.Success();

        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Unexpected error during home service initialization");
            CurrentHome = null; 
            return await errorFactory.FromApiExceptionAsync(ex, "HomeServiceInitializationFailedUnexpectedly");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<Result> CreateHome(string name, string icon, double? lattitude, double? longitude)
    {
        try
        {
            var request = new CreateHomeRequest(null, name, icon, null, null);

            if (lattitude is not null && longitude is not null)
            {
                request = new CreateHomeRequest(null, name, icon, lattitude, longitude);
            }

            await integrationService.CreateHome(request);

            return Result.Success();
        }
        catch (ApiException ex) 
        {
            logger.LogError(ex, "Failed to create home");
            return await errorFactory.FromApiExceptionAsync(ex,"FailedToCreateHome");
        }
    }

    public async Task<Result> JoinHome(string code)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateHomeDataSources(string dataSourceId)
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateHome(string name, string icon, double? lattitude, double? longitude)
    {
        throw new NotImplementedException();
    }

    #region IRoomService

    public async Task<Result> LoadRooms()
    {
        try
        {
            var homeId = CurrentHome!.Id;

            var roomsResponse = await roomsIntegrationService.DisplayRoomsAsync(homeId);

            _rooms = roomsResponse.Items;

            return Result.Success();
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("No rooms in home");
            _rooms = [];
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to load rooms");
            return await errorFactory.FromApiExceptionAsync<List<RoomDto>>(ex, "FailedToLoadRooms");
        }
    }

    public async Task<Result> CreateRoom(string name)
    {
        try
        {
            var homeId = CurrentHome!.Id;

            var dto = new CreateRoomRequest(name);

            await roomsIntegrationService.CreateRoomAsync(homeId, dto);

            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to add room");
            return await errorFactory.FromApiExceptionAsync(ex, "FailedToCreateRoom");
        }
    }

    public async Task<Result> UpdateRoom(string id, string name)
    {
        try
        {
            var homeId = CurrentHome!.Id;

            var dto = new UpdateRoomRequest(name);

            await roomsIntegrationService.UpdateRoomAsync(homeId, id, dto);

            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to update rooms");
            return await errorFactory.FromApiExceptionAsync(ex, "FailedToUpdateRoom");
        }
    }

    public async Task<Result> DeleteRoom(string id)
    {
        try
        {
            var homeId = CurrentHome!.Id;

            await roomsIntegrationService.DeleteRoomAsync(homeId, id);

            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to delete room");
            return await errorFactory.FromApiExceptionAsync(ex, "FailedToDeleteRoom");
        }
    }

    #endregion
}
