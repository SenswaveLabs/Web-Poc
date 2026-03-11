using MudBlazor;

namespace Senswave.Web.Themes;

public interface IThemeService
{
    MudTheme CurrentTheme { get; }

    string ThemeClass { get; }

    event Action? OnChange;

    Task SetCurrentTheme(Theme types);

    Task Initialize();
}
