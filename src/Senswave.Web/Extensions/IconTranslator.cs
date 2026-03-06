using MudBlazor;
using System.Text;
using System.Text.RegularExpressions;

public static class IconTranslator
{
    private static readonly Regex VariantRegex = new("-(outline|sharp)$", RegexOptions.Compiled);

    public static string IonToMud(string ionIcon)
    {
        if (string.IsNullOrWhiteSpace(ionIcon))
            return Icons.Material.Filled.Help;

        // remove ionicon variants
        var baseName = VariantRegex.Replace(ionIcon, "");

        // convert kebab-case → PascalCase
        var pascal = ToPascalCase(baseName);

        // attempt to resolve dynamically
        var field = typeof(Icons.Material.Filled).GetField(pascal);

        if (field != null)
            return (string)field.GetValue(null)!;

        return Icons.Material.Filled.Help;
    }

    private static string ToPascalCase(string kebab)
    {
        var parts = kebab.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var part in parts)
            sb.Append(char.ToUpperInvariant(part[0]) + part[1..]);

        return sb.ToString();
    }
}