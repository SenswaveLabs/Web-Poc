using Refit;
using Senswave.Web.DataSources.Integration;
using Senswave.Web.DataSources.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Services;

public class BrokerService(
    ILogger<BrokerService> logger, 
    IErrorFactory errorFactory, 
    IBrokerIntegrationService integrationService) : IBrokerService
{
    public async Task<Result<List<BrokerDto>>> GetBrokers()
    {
        logger.LogInformation("Getting brokers for user");

        try
        {
            var response = await integrationService.GetBrokersAsync();
            return Result<List<BrokerDto>>.Success(response.Items);
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<List<BrokerDto>>.Success([]);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get brokers");
            return await errorFactory.FromApiExceptionAsync<List<BrokerDto>>(ex, "GetBrokersFailed");
        }
    }

    public async Task<Result<BrokerModel>> GetBroker(string id)
    {
        try
        {
            var response = await integrationService.GetBrokerAsync(id);
            return Result<BrokerModel>.Success(response);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get broker {BrokerId}", id);
            return await errorFactory.FromApiExceptionAsync<BrokerModel>(ex, "GetBrokerFailed");
        }
    }

    public async Task<Result> AddBroker(CreateBrokerModel model)
    {
        try
        {
            await integrationService.CreateBrokerAsync(model);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to create broker");
            return await errorFactory.FromApiExceptionAsync(ex, "CreateBrokerFailed");
        }
    }

    public async Task<Result> UpdateBroker(string id, UpdateBrokerModel model)
    {
        try
        {
            await integrationService.UpdateBrokerAsync(id, model);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to update broker");
            return await errorFactory.FromApiExceptionAsync(ex, "UpdateBrokerFailed");
        }
    }

    public async Task<Result> DeleteBroker(string id)
    {
        try
        {
            await integrationService.DeleteBrokerAsync(id);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to delete broker {BrokerId}", id);
            return await errorFactory.FromApiExceptionAsync(ex, "DeleteBrokerFailed");
        }
    }

    public async Task<Result> StartClient(string id, string username, string password)
    {
        try
        {
            var request = new StartClientDto(username, password);
            await integrationService.StartClientAsync(id, request);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to start client for broker {BrokerId}", id);
            return await errorFactory.FromApiExceptionAsync(ex, "StartClientFailed");
        }
    }

    public async Task<Result> StopClient(string id)
    {
        try
        {
            await integrationService.StopClientAsync(id);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to stop client for broker {BrokerId}", id);
            return await errorFactory.FromApiExceptionAsync(ex, "StopClientFailed");
        }
    }

    public async Task<Result> RestartClient(string id)
    {
        try
        {
            await integrationService.RestartClientAsync(id);
            return Result.Success();
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to restart client for broker {BrokerId}", id);
            return await errorFactory.FromApiExceptionAsync(ex, "RestartClientFailed");
        }
    }
}
