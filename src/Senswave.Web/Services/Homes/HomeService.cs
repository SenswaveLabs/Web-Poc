using Senswave.Web.Homes.Models;
using Senswave.Web.Homes.Services;
using Senswave.Web.Integration.Homes;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Services.Homes;

public class HomeService(
    IErrorFactory errorFactory,
    IHomesIntegrationService integrationService, 
    ILogger<HomeService> logger) : IHomeService
{
    private HomeDetails CurrentHome { get; set; } = new();

    public async Task<Result> Initialize

    public async Task<Result<HomeDetails>> GetHome(string id)
    {
        logger.LogInformation("Getting homes for user");

        try
        {
            if (!string.IsNullOrEmpty(CurrentHome.Id))
                return Result<HomeDetails>.Success(CurrentHome);

            var response = await integrationService.GetHome(id);

            if (response is null)
            {
                logger.LogWarning("Get home failed for home {HomeId}: No response from integration service", id);
                return errorFactory.Create<HomeDetails>("GetHomeFailedUnexpectedly");
            }

            CurrentHome = (response as HomeDetails)!;

            return Result<HomeDetails>.Success(CurrentHome!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get homes for user");
            return errorFactory.Create<HomeDetails>("GetHomesFailed");
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
        catch (Exception ex) 
        {
            logger.LogError(ex, "Failed to get homes for user");
            return errorFactory.Create<List<Home>>("GetHomesFailed");
        }
    }
}
