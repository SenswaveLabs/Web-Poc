using Senswave.Web.Homes.Models;
using Senswave.Web.Homes.Services;
using Senswave.Web.Integration.Homes;
using Senswave.Web.Integration.Homes.Response;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Services.Homes;

public class HomeService(
    IErrorFactory errorFactory,
    IHomesIntegrationService integrationService,
    ILogger<HomeService> logger) : IHomeService
{
    private bool _initialized = false;

    private HomeDetails? _currentHome;

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

    public event Action? OnChange;

    public async Task<Result> Initialize()
    {
        await _initLock.WaitAsync();

        try
        {
            if (_initialized)
                return CurrentHome == null ? errorFactory.Create("InitializedWithNoHome") : Result.Success();

            _initialized = true;

            logger.LogInformation("Initializing home.");

            string currentHomeId = string.Empty;

            try
            {
                var currentHomeResponse = await integrationService.GetCurrentHome();

                if (currentHomeResponse is null || string.IsNullOrEmpty(currentHomeResponse.Id))
                {
                    logger.LogWarning("Get current home failed: No response from integration service");
                    CurrentHome = null;
                    return errorFactory.Create("GetCurrentHomeFailedUnexpectedly");
                }

                currentHomeId = currentHomeResponse.Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get current home for user");
                CurrentHome = null;
                return errorFactory.Create("GetCurrentHomeFailedUnexpectedly");
            }

            try
            {
                var response = await integrationService.GetHome(currentHomeId);

                if (response is null)
                {
                    logger.LogWarning("Get home failed for home {HomeId}: No response from integration service", currentHomeId);
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
                        Id = response.DataSource.Id,
                        Name = response.DataSource.Name,
                        State = response.DataSource.State
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get homes for user");
                _currentHome = null;
                return errorFactory.Create("FailedToLoadCurrentHomePleaseRefresh", "Failed to load current home please refresh.");
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

    public async Task<Result> ChangeHome(string newHomeId)
    {
        throw new NotImplementedException("Changing home is not implemented yet.");
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get homes for user");
            return errorFactory.Create<List<Home>>("GetHomesFailed");
        }
    }
}
