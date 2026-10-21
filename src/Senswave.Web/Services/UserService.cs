using Microsoft.AspNetCore.Components.Authorization;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Themes;
using Senswave.Web.Users.Auth.Services;
using Senswave.Web.Users.Users.Integration;
using Senswave.Web.Users.Users.Models;
using Senswave.Web.Users.Users.Services;

namespace Senswave.Web.Services;

public class UserService : IUserService
{
    private readonly IAuthenticationService authenticationService;
    private readonly IErrorFactory errorFactory;
    private readonly IUserIntegrationService userService;
    private readonly IThemeService themeService;
    private readonly ILogger<UserService> logger;

    private Task<Result>? _initialization;

    public UserService(
        IAuthenticationService authenticationService,
        AuthenticationStateProvider authenticationStateProvider,
        IErrorFactory errorFactory,
        IUserIntegrationService userService,
        IThemeService themeService,
        ILogger<UserService> logger)
    {
        this.authenticationService = authenticationService;
        this.errorFactory = errorFactory;
        this.userService = userService;
        this.themeService = themeService;
        this.logger = logger;

        // Login or logout means a different (or no) user, so cached details must be fetched again.
        authenticationStateProvider.AuthenticationStateChanged += _ => Reset();
    }

    public UserDetails Details { get; private set; } = new();

    public async Task<Result> Initialize()
    {
        // Concurrent callers share one request, so nobody reads Details before they are loaded.
        _initialization ??= InitializeInternal();

        var result = await _initialization;

        if (result.IsFailure)
            _initialization = null;

        return result;
    }

    private async Task<Result> InitializeInternal()
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

    private void Reset()
    {
        _initialization = null;
        Details = new();
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

    public async Task<Result> AcceptConsents()
    {
        try
        {
            await userService.AcceptConsents();

            Details.HasActiveConsent = true;

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to accept consents");
            return errorFactory.Create("Failed to accept terms and privacy policy.");
        }
    }

    public async Task<Result<LegalDocumentResponse>> GetTermsAndConditions()
    {
        try
        {
            return Result<LegalDocumentResponse>.Success(await userService.GetTermsAndConditions());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch terms and conditions");
            return errorFactory.Create<LegalDocumentResponse>("Failed to fetch terms and conditions.");
        }
    }

    public async Task<Result<LegalDocumentResponse>> GetPrivacyPolicy()
    {
        try
        {
            return Result<LegalDocumentResponse>.Success(await userService.GetPrivacyPolicy());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch privacy policy");
            return errorFactory.Create<LegalDocumentResponse>("Failed to fetch privacy policy.");
        }
    }
}
