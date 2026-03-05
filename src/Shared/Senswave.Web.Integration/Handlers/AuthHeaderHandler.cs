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

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
