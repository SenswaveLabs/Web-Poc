using Senswave.Web.Integration.Users;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Themes;
using Senswave.Web.Users.Users.Models;
using Senswave.Web.Users.Users.Services;

namespace Senswave.Web.Services.Users;

public class UserService(
    IErrorFactory errorFactory,
    IUserIntegrationService userService, 
    IThemeService themeService, 
    ILogger<UserService> logger) : IUserService
{
    public UserDetails Details { get; private set; } = new();

    public async Task<Result> Initialize()
    {
        try
        {
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

    public async Task<Result> OverrideSettings(string theme, string language)
    {
        try
        {
            var parsedTheme = theme.ToThemeEnum();
            var settings = new UserSettingUpdateRequest
            {
                Language = language,
                Theme = theme
            };

            await userService.UpdateSettings(settings);

            Details.Theme = settings.Theme;
            Details.Language = settings.Language;

            await themeService.SetCurrentTheme(parsedTheme);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to override user settings");
            return errorFactory.Create("Failed to override user settings.");
        }
    }
}
