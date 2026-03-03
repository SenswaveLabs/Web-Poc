namespace Senswave.Integration.Rest.Auth.Response;

public class LoginTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
