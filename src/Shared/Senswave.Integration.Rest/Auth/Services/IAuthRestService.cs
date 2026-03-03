using Refit;
using Senswave.Integration.Rest.Auth.Request;
using Senswave.Integration.Rest.Auth.Response;


namespace Senswave.Integration.Rest.Auth.Services;

public interface IAuthRestService
{
    [Post("/api/v1/auth/login")]
    Task<LoginTokenResponse> Login([Body] LoginRequest request);


    [Post("/api/v1/auth/login/google")]
    Task<LoginTokenResponse> LoginGoogle([Body] LoginGoogleRequest request);
}
