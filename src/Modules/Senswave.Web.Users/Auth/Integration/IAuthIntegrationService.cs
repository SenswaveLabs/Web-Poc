using Refit;
using Senswave.Web.Users.Auth.Models;

namespace Senswave.Web.Users.Auth.Integration;

public interface IAuthIntegrationService
{
    [Post("/api/v1/auth/login")]
    Task<LoginTokenResponse> Login([Body] LoginRequest request);

    [Post("/api/v1/auth/login/google")]
    Task<LoginTokenResponse> LoginGoogle([Body] LoginGoogleRequest request);

    [Post("/api/v1/auth/refresh")]
    Task<LoginTokenResponse> Refresh([Body] RefreshTokenRequest request);
}
