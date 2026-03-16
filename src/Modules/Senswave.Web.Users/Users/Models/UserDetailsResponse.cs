namespace Senswave.Web.Users.Users.Models;

public class UserDetailsResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;

    public bool HasActiveConsent { get; set; } = false;
}
