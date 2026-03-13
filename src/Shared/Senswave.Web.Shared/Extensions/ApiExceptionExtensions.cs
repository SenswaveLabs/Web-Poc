using Microsoft.Extensions.Logging;
using Refit;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Shared.Extensions;

public static class ApiExceptionExtensions
{
    public static async Task<Result> ToResultAsync(
        this ApiException ex,
        IErrorFactory errorFactory,
        ILogger logger,
        string fallbackCode = "RequestFailed")
    {
        try
        {
            var error = await ex.GetContentAsAsync<ApiErrorResponse>();

            var firstError = error?.Errors?.FirstOrDefault();

            if (firstError != null)
                return errorFactory.Create(firstError.Code, firstError.Description);
        }
        catch (Exception deserializeEx)
        {
            logger.LogError(deserializeEx, "Failed to deserialize API error response");
        }

        return errorFactory.Create(fallbackCode);
    }
}
