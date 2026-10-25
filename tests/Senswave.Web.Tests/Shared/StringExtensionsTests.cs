using MudBlazor;
using Senswave.Web.Shared.Extensions;

namespace Senswave.Web.Tests.Shared;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("radio-outline", Icons.Material.Rounded.Radio)]
    [InlineData("home-outline", Icons.Material.Rounded.Home)]
    [InlineData("bulb-sharp", Icons.Material.Rounded.Lightbulb)]
    [InlineData("thermostat", Icons.Material.Rounded.Thermostat)]
    [InlineData("HOME", Icons.Material.Rounded.Home)]
    public void TranslateIcon_maps_web_and_ionicons_names(string name, string expected)
    {
        Assert.Equal(expected, name.TranslateIcon());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown-icon-outline")]
    public void TranslateIcon_falls_back_to_generic_device_icon(string? name)
    {
        Assert.Equal(StringExtensions.FallbackIcon, name.TranslateIcon());
    }

    [Theory]
    [InlineData("radio-outline", "Radio")]
    [InlineData("hardware-chip-outline", "Hardware chip")]
    [InlineData("thermostat", "Thermostat")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ToIconLabel_returns_readable_name(string? name, string expected)
    {
        Assert.Equal(expected, name.ToIconLabel());
    }
}
