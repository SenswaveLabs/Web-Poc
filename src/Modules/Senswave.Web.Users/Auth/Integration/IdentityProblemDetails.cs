using Refit;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Users.Auth.Models;

namespace Senswave.Web.Users.Auth.Integration;

/// <summary>
/// Problem details returned by ASP.NET Core Identity endpoints (MapIdentityApi).
/// Shape differs from Senswave errors: <c>errors</c> is a dictionary and login failures use <c>detail</c>.
/// </summary>
public class IdentityProblemDetails
{
    public string? Detail { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }

    public static async Task<Error[]> ToErrors(ApiException exception)
    {
        IdentityProblemDetails? problem = null;

        try
        {
            problem = await exception.GetContentAsAsync<IdentityProblemDetails>();
        }
        catch
        {
            // Body is not problem details (e.g. proxy error page). Fall through to unknown error.
        }

        if (problem?.Errors is { Count: > 0 })
        {
            return problem.Errors
                .Select(x => new Error(x.Key, x.Value.FirstOrDefault()))
                .ToArray();
        }

        var code = problem?.Detail switch
        {
            "NotAllowed" => AuthErrorCodes.EmailNotConfirmed,
            "LockedOut" => AuthErrorCodes.LockedOut,
            "Failed" => AuthErrorCodes.InvalidCredentials,
            _ => AuthErrorCodes.Unknown
        };

        return [new Error(code)];
    }
}
