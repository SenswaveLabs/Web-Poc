namespace Senswave.Web.Themes;

public static class ThemeExtensions
{
    public static string ToThemeString(this Theme theme)
        => theme switch
        {
            Theme.Light => "light",
            Theme.Dark => "dark",
            Theme.Default => "default",
            Theme.HighContrast => "highcontrast",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };

    public static Theme ToThemeEnum(this string theme)
        => theme switch
        {
            "light" => Theme.Light,
            "dark" => Theme.Dark,
            "default" => Theme.Default,
            "highcontrast" => Theme.HighContrast,
            _ => Theme.Default
        };
}
