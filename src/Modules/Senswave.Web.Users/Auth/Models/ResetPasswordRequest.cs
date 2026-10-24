namespace Senswave.Web.Users.Auth.Models;

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string ResetCode { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
