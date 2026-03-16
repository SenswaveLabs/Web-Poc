using Microsoft.Extensions.Logging;
using Refit;

namespace Senswave.Web.Shared.Resulting;

public class ErrorFactory(ILogger<ErrorFactory> logger) : IErrorFactory
{
    public Result Create(string code, string? defaultDescription = null) => Result.Failure(new Error(code, defaultDescription));
    public Result<T> Create<T>(string code, string? defaultDescription = null)  => Result<T>.Failure(new Error(code, defaultDescription));

    public async Task<Result> FromApiExceptionAsync(ApiException exception, string fallbackCode)
    {
        var error = await TryExtractError(exception);

        if (error != null)
        {
            if (error.Description.Contains("No description"))
            {
                return Create(error.Code);
            }

            return Create(error.Code, error.Description);
        }

        return Create(fallbackCode);
    }

    public async Task<Result<T>> FromApiExceptionAsync<T>(ApiException exception, string fallbackCode)
    {
        var error = await TryExtractError(exception);

        if (error != null)
        {
            if (error.Description.Contains("No description"))
            {
                return Create<T>(error.Code);
            }

            return Create<T>(error.Code, error.Description);
        }

        return Create<T>(fallbackCode);
    }

    private async Task<ApiError?> TryExtractError(ApiException exception)
    {
        try
        {
            var response = await exception.GetContentAsAsync<ApiErrorResponse>();
            return response?.Errors?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize API error response");
            return null;
        }
    }
}
