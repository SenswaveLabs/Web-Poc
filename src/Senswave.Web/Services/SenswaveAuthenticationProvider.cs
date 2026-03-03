using Microsoft.AspNetCore.Components.Authorization;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Shared.Services;
using Senswave.Web.Users.Auth.Models;
using Senswave.Web.Users.Auth.Services;
using System.Security.Claims;

namespace Senswave.Web.Services;

public class SenswaveAuthenticationProvider(ILogger<SenswaveAuthenticationProvider> logger) : AuthenticationStateProvider, IAuthenticationService, ITokenStore
{
    #region AuthenticationStateProvider

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>();

        claims.Add(new Claim(ClaimTypes.Name, "Test User"));

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"))));
    }

    #endregion

    #region IAuthenticationService

    public Task<Result> Login(LoginWithPasswordModel model)
    {
        throw new NotImplementedException();
    }

    public Task<Result> Login(LoginWithGoogleModel model)
    {
        throw new NotImplementedException();
    }

    public Task<Result> Logout()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region ITokenStore

    public Task<Result<string>> GetBearerToken()
    {
        // todo refresh token if expired with lcok
        throw new NotImplementedException();
    }

    #endregion
}
