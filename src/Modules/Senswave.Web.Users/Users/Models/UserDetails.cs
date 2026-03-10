namespace Senswave.Web.Services.Users;

public class UserDetails
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Theme { get; set; } = string.Empty;
    
    public string Language { get; set; } = string.Empty;

    public bool HasActiveConsent { get; set; } = false;
}
