using Microsoft.AspNetCore.Components;

namespace Senswave.Web.Components.Auth;

public static class AuthRedirect
{
    /// <summary>
    /// Page to open after sign in: the local "returnUrl" query value of the current address, or "/".
    /// Only local paths are accepted, so a crafted link cannot send the user to another site.
    /// </summary>
    public static string SafeReturnUrl(NavigationManager navigation)
    {
        var returnUrl = new Uri(navigation.Uri).Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(pair => pair.StartsWith("returnUrl=", StringComparison.OrdinalIgnoreCase))
            .Select(pair => Uri.UnescapeDataString(pair["returnUrl=".Length..]))
            .FirstOrDefault();

        if (string.IsNullOrEmpty(returnUrl))
            return "/";

        var isLocal = returnUrl.StartsWith('/')
            && !returnUrl.StartsWith("//")
            && !returnUrl.StartsWith("/\\");

        return isLocal && !returnUrl.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)
            ? returnUrl
            : "/";
    }
}
