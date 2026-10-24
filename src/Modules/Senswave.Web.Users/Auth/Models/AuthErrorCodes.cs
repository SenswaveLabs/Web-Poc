namespace Senswave.Web.Users.Auth.Models;

/// <summary>
/// Error codes returned by auth services. Identity codes (DuplicateEmail, InvalidToken, Password*) are passed through unchanged.
/// </summary>
public static class AuthErrorCodes
{
    public const string InvalidCredentials = "InvalidCredentials";

    public const string EmailNotConfirmed = "EmailNotConfirmed";

    public const string LockedOut = "LockedOut";

    public const string Unknown = "Unknown";
}
