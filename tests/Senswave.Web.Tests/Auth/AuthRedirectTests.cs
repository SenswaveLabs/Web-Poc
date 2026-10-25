using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Senswave.Web.Components.Auth;

namespace Senswave.Web.Tests.Auth;

public class AuthRedirectTests : BunitContext
{
    private string ReturnUrlFor(string address)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(address);
        return AuthRedirect.SafeReturnUrl(navigation);
    }

    [Theory]
    [InlineData("/auth/login?returnUrl=%2Fusers%2Fdetails", "/users/details")]
    [InlineData("/auth/login?passwordReset=true&returnUrl=%2Fdevices%2Fdashboard%2F42", "/devices/dashboard/42")]
    public void Returns_local_return_url(string address, string expected)
    {
        Assert.Equal(expected, ReturnUrlFor(address));
    }

    [Theory]
    [InlineData("/auth/login")]
    [InlineData("/auth/login?returnUrl=")]
    [InlineData("/auth/login?returnUrl=https%3A%2F%2Fevil.example.com")]
    [InlineData("/auth/login?returnUrl=%2F%2Fevil.example.com")]
    [InlineData("/auth/login?returnUrl=%2F%5Cevil.example.com")]
    [InlineData("/auth/login?returnUrl=%2Fauth%2Fregister")]
    public void Falls_back_to_root_for_missing_external_or_auth_urls(string address)
    {
        Assert.Equal("/", ReturnUrlFor(address));
    }
}
