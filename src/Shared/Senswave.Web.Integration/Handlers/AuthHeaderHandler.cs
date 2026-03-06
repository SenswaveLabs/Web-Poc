using Microsoft.Extensions.Logging;
using Senswave.Web.Shared.Services;
using System.Net.Http.Headers;

namespace Senswave.Web.Integration.Handlers;

public class AuthHeaderHandler(
    ITokenStore tokenStore,
    ILogger<AuthHeaderHandler> logger) : DelegatingHandler

{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenRequest = await tokenStore.GetBearerToken();

        if (tokenRequest.IsFailure)
        {
            logger.LogError("Failed to retrieve bearer token: {Errors}", tokenRequest.Errors);

            throw new NotImplementedException("Failed to append token.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenRequest.Value);

        var firstAttemptResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (firstAttemptResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Received 401 Unauthorized. Attempting to refresh token and retry request.");
            var refreshResult = await tokenStore.RefreshToken();

            if (refreshResult.IsFailure)
            {
                logger.LogError("Failed to refresh token: {Errors}", refreshResult.Errors);
                return firstAttemptResponse;
            }

            var newTokenRequest = await tokenStore.GetBearerToken();

            if (newTokenRequest.IsFailure)
            {
                logger.LogError("Failed to retrieve new bearer token after refresh: {Errors}", newTokenRequest.Errors);
                return firstAttemptResponse;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokenRequest.Value);
            logger.LogInformation("Retrying request with refreshed token.");

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return firstAttemptResponse;
    }
}
