using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Shared.Services;
using Senswave.Web.Users.Auth.Integration;
using Senswave.Web.Users.Auth.Models;
using Senswave.Web.Users.Auth.Services;
using System.Security.Claims;

namespace Senswave.Web.Services;


public class SenswaveAuthenticationProvider(
    NavigationManager navigation,
    IAuthIntegrationService authIntegrationService,
    ILocalStorageService localStorageService,
    ISessionStorageService sessionStorageService,
    IErrorFactory errorFactory,
    ILogger<SenswaveAuthenticationProvider> logger) : AuthenticationStateProvider, IAuthenticationService, ITokenStore
{
    private const string RememberMeKey = "senswave-remember-me";

    private const string AccessTokenKey = "senswave-access-token";

    private const string RefreshTokenKey = "senswave-refresh-token";

    private bool _initialized = false;

    private string _accessToken = string.Empty;
    
    private string _refreshToken = string.Empty;

    private DateTime _expiresIn = DateTime.UtcNow;

    #region AuthenticationStateProvider

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Initialize();

        if (string.IsNullOrEmpty(_accessToken))
        {
            logger.LogInformation("No access token found, user is not authenticated.");
            await FullLogout();
            return await NoAuthState();
        }

        logger.LogDebug("Access token found, user is authenticated");
        return await AuthenticatedState("User");
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

            await InitializeSession(response.AccessToken, response.RefreshToken, response.ExpiresIn, model.RememberMe);

            NotifyAuthenticationStateChanged(AuthenticatedState("User"));

            logger.LogInformation("User {Username} logged in successfully. Access token expires at {ExpiresIn}. Remember me: {RememberMe}", model.Username, _expiresIn, model.RememberMe);
            return Result.Success();
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Login failed for user {Username}", model.Username);

            return errorFactory.Create("LoginFailed");
        }
    }

    public async Task<Result> Login(string token)
    {
        logger.LogInformation("Attempting to log in user with google.");

        try
        {
            var request = new LoginGoogleRequest
            {
                Token = token
            };

            var response = await authIntegrationService.LoginWithGoogleToken(request);

            await InitializeSession(response.AccessToken, response.RefreshToken, response.ExpiresIn, true);

            NotifyAuthenticationStateChanged(AuthenticatedState("User"));

            logger.LogInformation("User logged in successfully. Access token expires at {ExpiresIn}.", _expiresIn);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google login failed for user.");

            return errorFactory.Create("LoginFailed");
        }
    }

    public async Task<Result> Logout()
    {
        try
        {
            await FullLogout();
            NotifyAuthenticationStateChanged(NoAuthState());

            logger.LogInformation("User logged out successfully");
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

    public async Task<Result<string>> GetBearerToken()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            logger.LogInformation("No access token available");
            return errorFactory.Create<string>("NoAccessToken");
        }

        return Result<string>.Success(_accessToken);
    }

    public async Task<Result> RefreshToken()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            logger.LogInformation("No refresh token available");

            await FullLogout();
            NotifyAuthenticationStateChanged(NoAuthState());
            return errorFactory.Create("NoRefreshToken");
        }

        return await InternalRefresh();
    }

    private async Task<Result> InternalRefresh()
    {
        try
        {
            var response = await authIntegrationService.Refresh(new RefreshTokenRequest
            {
                RefreshToken = _refreshToken
            });

            if (response is null)
            {
                logger.LogWarning("Token refresh failed. No response from authentication service");

                await FullLogout();
                return errorFactory.Create("TokenRefreshFailedUnexpectedly");
            }

            var rememberMeResult = await localStorageService.Get<bool>(RememberMeKey);

            var rememberMe = rememberMeResult.IsSuccess && rememberMeResult.Value;

            await InitializeSession(response.AccessToken, response.RefreshToken, response.ExpiresIn, rememberMe);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token refresh failed");
            return errorFactory.Create("TokenRefreshFailed");
        }
    }

    #endregion

    private async Task Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        var rememberMeResult = await localStorageService.Get<bool>(RememberMeKey);

        if (!rememberMeResult.Value)
        {
            logger.LogInformation("Remember me was disabled");
            return;
        }

       
        var accessTokenResult = await localStorageService.Get<string>(AccessTokenKey);
        var refreshTokenResult = await localStorageService.Get<string>(RefreshTokenKey);

        if (accessTokenResult.IsFailure || refreshTokenResult.IsFailure)
        {
            logger.LogInformation("Failed to load tokens from local storage. Access token found: {AccessTokenFound}, Refresh token found: {RefreshTokenFound}", 
                accessTokenResult.IsSuccess, 
                refreshTokenResult.IsSuccess);
            return;
        }

        logger.LogInformation("Loaded tokens from local storage.");

        _accessToken = accessTokenResult.Value;
        _refreshToken = refreshTokenResult.Value;

        var result = await InternalRefresh();

        logger.LogInformation("Token refresh on initialization completed. Success: {Success}", result.IsSuccess);

        if (result.IsSuccess)
        {
            navigation.NavigateTo("/");
        }
    }

    private async Task InitializeSession(string accessToken, string refreshToken, int expiresIn, bool rememberMe)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _expiresIn = DateTime.UtcNow.AddSeconds(expiresIn-30);

        if (rememberMe)
        {
            await localStorageService.Set(AccessTokenKey, accessToken);
            await localStorageService.Set(RefreshTokenKey, refreshToken);
            await localStorageService.Set(RememberMeKey, true);
        }
        else
        {
            await sessionStorageService.Set(AccessTokenKey, accessToken);
            await sessionStorageService.Set(RefreshTokenKey, refreshToken);
            await localStorageService.Set(RememberMeKey, false);
        }

        logger.LogInformation("Tokens saved. Access token expires at {ExpiresIn}. Remember me: {RememberMe}", expiresIn, rememberMe);
    }

    private async Task FullLogout()
    {
        _accessToken = string.Empty;
        _refreshToken = string.Empty;
        _expiresIn = DateTime.UtcNow;

        await localStorageService.Remove(AccessTokenKey);
        await localStorageService.Remove(RefreshTokenKey);
        await localStorageService.Remove(RememberMeKey);
        await sessionStorageService.Remove(AccessTokenKey);
        await sessionStorageService.Remove(RefreshTokenKey);
    }

    private static Task<AuthenticationState> NoAuthState() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private static Task<AuthenticationState> AuthenticatedState(string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"))));
    }
}
