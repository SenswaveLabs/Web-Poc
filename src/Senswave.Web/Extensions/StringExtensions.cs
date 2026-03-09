using MudBlazor;
using System.Reflection;
using System.Text;

namespace Senswave.Web.Extensions;

public static class StringExtensions
{
    private static readonly Type MaterialType = typeof(Icons.Material.Rounded);

    public static string TranslateIcon(this string ionIcon)
    {
        if (string.IsNullOrWhiteSpace(ionIcon))
            return Icons.Material.Rounded.HelpOutline;

        var normalized = NormalizeIonIcon(ionIcon);
        var pascalName = KebabToPascal(normalized);

        var property = MaterialType.GetField(
            pascalName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

        if (property != null)
            return property.GetValue(null)?.ToString() ?? Icons.Material.Rounded.HelpOutline;

        return Icons.Material.Rounded.HelpOutline;
    }

    private static string NormalizeIonIcon(string icon)
    {
        icon = icon.ToLower();

        if (icon.EndsWith("-outline"))
            icon = icon.Replace("-outline", "");

        if (icon.EndsWith("-sharp"))
            icon = icon.Replace("-sharp", "");

        return icon;
    }

    private static string KebabToPascal(string input)
    {
        var parts = input.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            sb.Append(char.ToUpper(part[0]) + part.Substring(1));
        }

        return sb.ToString();
    }
}
