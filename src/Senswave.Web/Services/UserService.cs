using Senswave.Web.Shared.Resulting;
using Senswave.Web.Themes;
using Senswave.Web.Users.Auth.Services;
using Senswave.Web.Users.Users.Integration;
using Senswave.Web.Users.Users.Models;
using Senswave.Web.Users.Users.Services;

namespace Senswave.Web.Services;

public class UserService(
    IAuthenticationService authenticationService,
    IErrorFactory errorFactory,
    IUserIntegrationService userService, 
    IThemeService themeService, 
    ILogger<UserService> logger) : IUserService
{
    private bool _initialized = false;

    public UserDetails Details { get; private set; } = new();

    public async Task<Result> Initialize()
    {
        if (_initialized)
            return Result.Success();

        try
        {
            _initialized = true;
            var userData = await userService.GetInfo();

            Details = new UserDetails
            {
                Id = userData.Id,
                Email = userData.Email,
                Theme = userData.Theme,
                Language = userData.Language,
                HasActiveConsent = userData.HasActiveConsent
            };

            var theme = userData.Theme.ToThemeEnum();

            await themeService.SetCurrentTheme(theme);

            return Result.Success();
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, "Failed to initialize UserService");
            return errorFactory.Create("Failed to fetch user data.");
        }
    }

    public async Task<Result> RemoveAccount()
    {
        try
        {
            await userService.DeleteAccount();

            return await authenticationService.Logout();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove user account");
            return errorFactory.Create("Failed to remove user account.");
        }
    }

    public async Task<Result> OverrideSettings(string? theme = null, string? language = null)
    {
        try
        {
            var settings = new UserSettingUpdateRequest();

            if (!string.IsNullOrEmpty(theme))
            {
                settings.Theme = theme;
            }

            if (!string.IsNullOrEmpty(language)) 
            {
                settings.Language = language;
            }

            await userService.UpdateSettings(settings);

            if (!string.IsNullOrEmpty(theme))
            {
                var parsedTheme = theme.ToThemeEnum();
                Details.Theme = theme;

                await themeService.SetCurrentTheme(parsedTheme);
            }

            if (!string.IsNullOrEmpty(language))
            {
                Details.Language = language; 
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to override user settings");
            return errorFactory.Create("Failed to override user settings.");
        }
    }
}
