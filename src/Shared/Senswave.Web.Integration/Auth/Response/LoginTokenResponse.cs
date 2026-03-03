namespace Senswave.Web.Integration.Auth.Response;

public class LoginTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; } = 0;
}
