using Microsoft.AspNetCore.Components.Authorization;
using Senswave.Web.Integration.Auth.Request;
using Senswave.Web.Integration.Auth.Services;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Shared.Services;
using Senswave.Web.Users.Auth.Models;
using Senswave.Web.Users.Auth.Services;
using System.Security.Claims;

namespace Senswave.Web.Services;

public class SenswaveAuthenticationProvider(
    IAuthIntegrationService authIntegrationService,
    IErrorFactory errorFactory,
    ILogger<SenswaveAuthenticationProvider> logger) : AuthenticationStateProvider, IAuthenticationService, ITokenStore
{

    private string _accessToken = string.Empty;
    
    private string _refreshToken = string.Empty;

    private DateTime _expiresIn = DateTime.UtcNow;

    #region AuthenticationStateProvider

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //TODO: Load from storage?

        if (string.IsNullOrEmpty(_accessToken))
        {
            logger.LogInformation("No access token found, user is not authenticated");

            return Task.FromResult(NoAuthState());
        }

        logger.LogDebug("Access token found, user is authenticated");

        return Task.FromResult(AuthenticatedState("User"));
    }

    private static AuthenticationState NoAuthState() => new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static AuthenticationState AuthenticatedState(string role) 
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };
        
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    #endregion

    #region IAuthenticationService

    public async Task<Result> Login(LoginWithPasswordModel model)
    {
        logger.LogInformation("Attempting to log in user {Username}", model.Username);

        try
        {
            var response = await authIntegrationService.Login(new LoginRequest
            {
                Email = model.Username,
                Password = model.Password
            });

            if (response == null) 
            {
                logger.LogWarning("Login failed for user {Username}: No response from authentication service", model.Username);

                return errorFactory.Create("LoginFailedUnexpectedly");
            }

            _accessToken = response.AccessToken;
            _refreshToken = response.RefreshToken;
            _expiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn-30);

            NotifyAuthenticationStateChanged(Task.FromResult(AuthenticatedState("User")));

            return Result.Success();
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Login failed for user {Username}", model.Username);

            return errorFactory.Create("LoginFailed");
        }
    }

    public Task<Result> Login(LoginWithGoogleModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Logout()
    {
        try
        {
            await Task.Delay(5);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout failed");
            return errorFactory.Create("LogoutFailed");
        }
    }

    #endregion

    #region ITokenStore

    public Task<Result<string>> GetBearerToken()
    {
        return Task.FromResult(Result<string>.Success(_accessToken));
    }

    #endregion
}
