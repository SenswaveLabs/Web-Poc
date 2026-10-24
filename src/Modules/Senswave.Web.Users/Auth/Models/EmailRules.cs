using System.ComponentModel.DataAnnotations;

namespace Senswave.Web.Users.Auth.Models;

public static class EmailRules
{
    private static readonly EmailAddressAttribute Attribute = new();

    /// <summary>
    /// Returns an error message or null. Empty input reports "required" instead of "invalid".
    /// </summary>
    public static string? Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Email is required.";

        return Attribute.IsValid(email) ? null : "The email address is invalid.";
    }
}
