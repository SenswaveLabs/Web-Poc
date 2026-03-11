using Microsoft.JSInterop;
using MudBlazor;
using Senswave.Web.Shared.Services;

namespace Senswave.Web.Themes;

internal sealed class ThemeService(
    ILocalStorageService localStorageService,
    IJSRuntime jsRuntime) : IThemeService
{
    private const string ThemeKey = "senswave-theme";

    private MudTheme _theme = Light;

    public string ThemeClass => CurrentTheme == HighContrast ? "high-contrast" : string.Empty;

    public MudTheme CurrentTheme
    {
        get => _theme;
        private set
        {
            _theme = value;
            OnChange?.Invoke();
        }
    }

    public event Action? OnChange;

    public async Task Initialize()
    {
        var result = await localStorageService.Get<string>(ThemeKey);

        if (result.IsSuccess && Enum.TryParse<Theme>(result.Value, out var theme))
        {
            await ApplyTheme(theme);
        }
        else
        {
            await ApplyTheme(Theme.Default);
        }
    }

    public async Task SetCurrentTheme(Theme type)
    {
        await localStorageService.Set(ThemeKey, type.ToString());
        await ApplyTheme(type);
    }

    private async Task ApplyTheme(Theme type)
    {
        if (type == Theme.Default)
        {
            var prefersDark = await jsRuntime.InvokeAsync<bool>(
                "eval",
                "window.matchMedia('(prefers-color-scheme: dark)').matches");

            CurrentTheme = prefersDark ? Dark : Light;
            
            return;
        }

        CurrentTheme = type switch
        {
            Theme.Light => Light,
            Theme.Dark => Dark,
            Theme.HighContrast => HighContrast,
            _ => Light
        };
    }

    public static MudTheme Light = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#EBAB17",      // Brand amber
            Secondary = "#00695C",    // Teal 800
            Tertiary = "#3949AB",     // Indigo 600

            Background = "#FAFAFA",
            Surface = "#FFFFFF",

            AppbarBackground = "#EBAB17",
            AppbarText = "#1A1A1A",

            DrawerBackground = "#FFFFFF",
            DrawerText = "rgba(0,0,0,0.87)",

            TextPrimary = "rgba(0,0,0,0.87)",
            TextSecondary = "rgba(0,0,0,0.60)",

            Success = "#2E7D32",
            Warning = "#F57C00",
            Error = "#D32F2F",
            Info = "#1976D2"
        }
    };

    public static MudTheme Dark = new()
    {
        PaletteLight = new PaletteDark()
        {
            Primary = "#F2C94C",      // Soft amber
            Secondary = "#4DB6AC",    // Teal 300
            Tertiary = "#9FA8DA",     // Indigo 200

            Background = "#121212",
            Surface = "#1E1E1E",

            AppbarBackground = "#1B1B1B",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#1B1B1B",
            DrawerText = "#FFFFFF",

            TextPrimary = "#FFFFFF",
            TextSecondary = "rgba(255,255,255,0.70)",

            Success = "#66BB6A",
            Warning = "#FFA726",
            Error = "#EF5350",
            Info = "#42A5F5"
        }
    };

    public static MudTheme HighContrast = new()
    {
        PaletteLight = new PaletteDark()
        {
            Primary = "#EBAB17",      // brand accent
            Secondary = "#00E5FF",    // bright for focus / borders
            Tertiary = "#7C4DFF",

            Background = "#000000",   // dark page
            Surface = "#121212",      // dark cards/panels

            AppbarBackground = "#000000",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#000000",
            DrawerText = "#FFFFFF",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#FFFFFF",

            Success = "#00E676",
            Warning = "#FFAB00",
            Error = "#FF1744",
            Info = "#00B0FF"
        }
    };
}