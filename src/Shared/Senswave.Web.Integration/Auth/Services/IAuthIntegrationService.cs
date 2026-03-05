using Refit;
using Senswave.Web.Integration.Auth.Request;
using Senswave.Web.Integration.Auth.Response;


namespace Senswave.Web.Integration.Auth.Services;

public interface IAuthIntegrationService
{
    [Post("/api/v1/auth/login")]
    Task<LoginTokenResponse> Login([Body] LoginRequest request);


    [Post("/api/v1/auth/login/google")]
    Task<LoginTokenResponse> LoginGoogle([Body] LoginGoogleRequest request);
}
