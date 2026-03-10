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
            Primary = "#3F51B5",        
            Secondary = "#009688",      
            Tertiary = "#FF9800",       

            Background = "#F5F5F5",
            Surface = "#FFFFFF",

            AppbarBackground = "#3F51B5",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#FFFFFF",
            DrawerText = "rgba(0,0,0,0.87)",

            TextPrimary = "rgba(0,0,0,0.87)",
            TextSecondary = "rgba(0,0,0,0.60)",

            Success = "#4CAF50",
            Warning = "#FF9800",
            Error = "#F44336",
            Info = "#2196F3"
        }
    };

    public static MudTheme Dark = new()
    {
        PaletteLight = new PaletteDark()
        {
            Primary = "#90CAF9",        
            Secondary = "#80CBC4",     
            Tertiary = "#FFB74D",     

            Background = "#121212",
            Surface = "#1E1E1E",

            AppbarBackground = "#1E1E1E",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#1E1E1E",
            DrawerText = "#FFFFFF",

            TextPrimary = "#FFFFFF",
            TextSecondary = "rgba(255,255,255,0.70)",

            Success = "#81C784",
            Warning = "#FFB74D",
            Error = "#EF9A9A",
            Info = "#64B5F6"
        }
    };

    public static MudTheme HighContrast = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#0000FF",
            Secondary = "#008000",
            Tertiary = "#FF8C00",

            Background = "#FFFFFF",
            Surface = "#FFFFFF",

            AppbarBackground = "#000000",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#000000",
            DrawerText = "#FFFFFF",

            TextPrimary = "#000000",
            TextSecondary = "#000000",

            Success = "#008000",
            Warning = "#FF8C00",
            Error = "#FF0000",
            Info = "#0000FF"
        }
    };
}