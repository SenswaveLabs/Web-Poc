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
            Primary = "#EBAB17",
            PrimaryContrastText = "#1A1A1A",
            Secondary = "#00695C",

            HoverOpacity=0.1,
            Background = "#F5F5F5",
            Surface = "#edebeb",
            DrawerBackground = "#edebeb",

            AppbarBackground = "#EBAB17",
            AppbarText = "#1A1A1A",

            TextPrimary = "#000000",
            TextSecondary = "#424242",

            ActionDefault = "#1A1A1A",
            Divider = "rgba(0,0,0,0.12)"
        }
    };

    public static MudTheme Dark = new()
    {
        PaletteLight = new PaletteDark()
        {
            Primary = "#FFC107",     
            PrimaryContrastText = "#000000", 
            Secondary = "#5FA8A2",    
            Tertiary = "#C5CAE9",     

            Background = "#121212",
            Surface = "#1E1E1E",

            AppbarBackground = "#1B1B1B",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#1B1B1B",
            DrawerText = "#FFFFFF",

            TextPrimary = "#E0E0E0",
            TextSecondary = "rgba(255,255,255,0.70)",

            Success = "#81C784",
            Warning = "#FFB74D",
            Error = "#E57373",
            Info = "#64B5F6"
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