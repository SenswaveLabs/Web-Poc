using System.ComponentModel.DataAnnotations;

namespace Senswave.Web.Users.Auth.Models;

public class LoginWithPasswordModel
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
