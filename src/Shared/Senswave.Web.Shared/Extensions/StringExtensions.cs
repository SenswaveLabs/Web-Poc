using MudBlazor;

namespace Senswave.Web.Shared.Extensions;

public static class StringExtensions
{
    public const string FallbackIcon = Icons.Material.Rounded.DevicesOther;

    // Explicit map instead of reflection over Icons.Material.Rounded: the Release WASM build
    // trims the unused icon constants, so a reflection lookup always fell back to the help icon.
    // Keys cover names chosen in the web app and Ionicons names used by the mobile app (without "-outline"/"-sharp").
    private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["home"] = Icons.Material.Rounded.Home,
        ["flat"] = Icons.Material.Rounded.HomeWork,
        ["work"] = Icons.Material.Rounded.HomeRepairService,
        ["business"] = Icons.Material.Rounded.Business,
        ["bed"] = Icons.Material.Rounded.Bed,
        ["settings"] = Icons.Material.Rounded.Settings,
        ["info"] = Icons.Material.Rounded.Info,
        ["add"] = Icons.Material.Rounded.Add,
        ["delete"] = Icons.Material.Rounded.Delete,
        ["edit"] = Icons.Material.Rounded.Edit,
        ["refresh"] = Icons.Material.Rounded.Refresh,
        ["search"] = Icons.Material.Rounded.Search,
        ["account-circle"] = Icons.Material.Rounded.AccountCircle,
        ["notifications"] = Icons.Material.Rounded.Notifications,
        ["help"] = Icons.Material.Rounded.Help,
        ["data-source"] = Icons.Material.Rounded.DeviceHub,
        ["thermostat"] = Icons.Material.Rounded.Thermostat,
        ["thermometer"] = Icons.Material.Rounded.Thermostat,
        ["lightbulb"] = Icons.Material.Rounded.Lightbulb,
        ["bulb"] = Icons.Material.Rounded.Lightbulb,
        ["flash"] = Icons.Material.Rounded.FlashOn,
        ["camera"] = Icons.Material.Rounded.Videocam,
        ["videocam"] = Icons.Material.Rounded.Videocam,
        ["lock"] = Icons.Material.Rounded.Lock,
        ["lock-closed"] = Icons.Material.Rounded.Lock,
        ["lock-open"] = Icons.Material.Rounded.LockOpen,
        ["key"] = Icons.Material.Rounded.Key,
        ["alarm"] = Icons.Material.Rounded.Alarm,
        ["fan"] = Icons.Material.Rounded.Toys,
        ["speaker"] = Icons.Material.Rounded.Speaker,
        ["volume-high"] = Icons.Material.Rounded.VolumeUp,
        ["music"] = Icons.Material.Rounded.MusicNote,
        ["musical-notes"] = Icons.Material.Rounded.MusicNote,
        ["radio"] = Icons.Material.Rounded.Radio,
        ["printer"] = Icons.Material.Rounded.Print,
        ["print"] = Icons.Material.Rounded.Print,
        ["tv"] = Icons.Material.Rounded.Tv,
        ["desktop"] = Icons.Material.Rounded.DesktopMac,
        ["laptop"] = Icons.Material.Rounded.Laptop,
        ["tablet"] = Icons.Material.Rounded.TabletMac,
        ["tablet-portrait"] = Icons.Material.Rounded.TabletMac,
        ["phone-portrait"] = Icons.Material.Rounded.Smartphone,
        ["watch"] = Icons.Material.Rounded.Watch,
        ["server"] = Icons.Material.Rounded.Dns,
        ["hardware-chip"] = Icons.Material.Rounded.Memory,
        ["cloud"] = Icons.Material.Rounded.Cloud,
        ["sensor"] = Icons.Material.Rounded.Sensors,
        ["pulse"] = Icons.Material.Rounded.MonitorHeart,
        ["analytics"] = Icons.Material.Rounded.Analytics,
        ["speedometer"] = Icons.Material.Rounded.Speed,
        ["wifi"] = Icons.Material.Rounded.Wifi,
        ["power"] = Icons.Material.Rounded.PowerSettingsNew,
        ["toggle"] = Icons.Material.Rounded.ToggleOn,
        ["water"] = Icons.Material.Rounded.WaterDrop,
        ["snow"] = Icons.Material.Rounded.AcUnit,
        ["sunny"] = Icons.Material.Rounded.WbSunny,
        ["moon"] = Icons.Material.Rounded.DarkMode,
        ["flame"] = Icons.Material.Rounded.LocalFireDepartment,
        ["leaf"] = Icons.Material.Rounded.Spa,
        ["battery-full"] = Icons.Material.Rounded.BatteryFull,
        ["car"] = Icons.Material.Rounded.DirectionsCar,
        ["construct"] = Icons.Material.Rounded.Construction,
        ["cube"] = Icons.Material.Rounded.ViewInAr,
        ["grid"] = Icons.Material.Rounded.GridView,
        ["apps"] = Icons.Material.Rounded.Apps,
        ["options"] = Icons.Material.Rounded.Tune,
    };

    public static string TranslateIcon(this string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
            return FallbackIcon;

        if (IconMap.TryGetValue(icon, out var exact))
            return exact;

        return IconMap.TryGetValue(NormalizeIonIcon(icon), out var normalized)
            ? normalized
            : FallbackIcon;
    }

    private static string NormalizeIonIcon(string icon)
    {
        icon = icon.ToLowerInvariant();

        foreach (var suffix in new[] { "-outline", "-outlined", "-filled", "-sharp" })
        {
            if (icon.EndsWith(suffix))
                return icon[..^suffix.Length];
        }

        return icon;
    }
}
