using Microsoft.JSInterop;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Shared.Services;
using System.Text.Json;

namespace Senswave.Web.Services.Shared;

public class SessionStorageService(
    IJSRuntime js, 
    IErrorFactory errorFactory,
    ILogger<SessionStorageService> logger) : ISessionStorageService
{
    public async ValueTask<Result<T>> Get<T>(string key)
    {
        try
        {
            var json = await js.InvokeAsync<string>(
                "sessionStorage.geItemt",
                key);

            if (string.IsNullOrEmpty(json))
                return errorFactory.Create<T>("SessionValueNotFound");

            logger.LogInformation("Retrieved value from session storage for key: {Key}", key);

            var value = JsonSerializer.Deserialize<T>(json);

            return Result<T>.Success(value!);
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Failed to get value from session storage for key: {Key}", key);

            return errorFactory.Create<T>("FailedToGetSessionValue");
        }
    }

    public async ValueTask<Result> Set<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            await js.InvokeVoidAsync(
                "sessionStorage.setItem",
                key,
                json);

            logger.LogInformation("Set value in session storage for key: {Key}", key);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set value in session storage for key: {Key}", key);

            return errorFactory.Create("FailedToStoreSessionValue");
        }
    }

    public async ValueTask<Result> Remove(string key)
    {
        try
        {
            await js.InvokeVoidAsync(
                "sessionStorage.removeItem",
                key);
    
            logger.LogInformation("Removed value from session storage for key: {Key}", key);
    
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove value from session storage for key: {Key}", key);
    
            return errorFactory.Create("FailedToRemoveSessionValue");
        }
    }
}
