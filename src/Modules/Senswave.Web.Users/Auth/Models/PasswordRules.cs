using System.Text.RegularExpressions;

namespace Senswave.Web.Users.Auth.Models;

/// <summary>
/// Mirrors backend Identity password options (UsersModule.cs). Keep both in sync.
/// </summary>
public static class PasswordRules
{
    public const int MinLength = 10;

    public const string Description = "At least 10 characters with an uppercase letter, a lowercase letter, a digit and a special character.";

    public static IEnumerable<string> Validate(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            yield return "Password is required.";
            yield break;
        }

        if (password.Length < MinLength)
            yield return $"Password must be at least {MinLength} characters long.";
        if (!Regex.IsMatch(password, "[A-Z]"))
            yield return "Password must contain an uppercase letter.";
        if (!Regex.IsMatch(password, "[a-z]"))
            yield return "Password must contain a lowercase letter.";
        if (!Regex.IsMatch(password, "[0-9]"))
            yield return "Password must contain a digit.";
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            yield return "Password must contain a special character.";
    }
}
