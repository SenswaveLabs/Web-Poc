using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Senswave.Web.Components.Navigation;
using Senswave.Web.Homes.Models;
using Senswave.Web.Homes.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Navigation;

public class NavigationComponentsTests : ComponentTestBase
{
    private readonly IHomeService _homes = Substitute.For<IHomeService>();

    public NavigationComponentsTests()
    {
        Services.AddSingleton(_homes);
        _homes.GetHomes().Returns(Result<List<Home>>.Success(new List<Home>
        {
            new() { Id = "h1", Name = "Test Home", Icon = "home-outline", IsOwner = true },
            new() { Id = "h2", Name = "Summer House", Icon = "flat", IsOwner = false }
        }));
        _homes.ChangeHome(Arg.Any<string>()).Returns(Result.Success());
    }

    private void SetCurrentHome(string? id) =>
        _homes.CurrentHome.Returns(id is null ? null : new HomeDetails { Id = id, Name = id == "h1" ? "Test Home" : "Summer House", Icon = "home" });

    [Fact]
    public void Switcher_without_home_links_to_home_list()
    {
        SetCurrentHome(null);

        var cut = Render<HomeSwitcher>();

        var link = cut.Find("a");
        Assert.Equal("/homes/list", link.GetAttribute("href"));
        Assert.Contains("Choose home", link.TextContent);
    }

    [Fact]
    public void Switcher_shows_current_home_and_lists_homes()
    {
        SetCurrentHome("h1");

        var cut = Render<HomeSwitcher>();
        Assert.Contains("Test Home", cut.Markup);

        cut.Find("button.home-switcher").Click();

        Popovers.WaitForAssertion(() =>
        {
            Assert.Contains("Summer House", Popovers.Markup);
            Assert.Contains("All homes", Popovers.Markup);
        });
    }

    [Fact]
    public void Switching_home_changes_it_and_reloads_dashboard()
    {
        SetCurrentHome("h1");
        var cut = Render<HomeSwitcher>();

        cut.Find("button.home-switcher").Click();
        Popovers.WaitForElements(".mud-menu-item")
            .First(e => e.TextContent.Contains("Summer House"))
            .Click();

        var navigation = Services.GetRequiredService<NavigationManager>();
        cut.WaitForAssertion(() => _homes.Received(1).ChangeHome("h2"));
        cut.WaitForAssertion(() => Assert.Equal("http://localhost/", navigation.Uri));
    }

    [Fact]
    public void Bottom_navigation_with_home_shows_dashboard_and_home()
    {
        SetCurrentHome("h1");

        var cut = Render<BottomNavigation>();

        var links = cut.FindAll("a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal(["/devices", "/homes/details", "/datasources", "/users/details"], links);
    }

    [Fact]
    public void Bottom_navigation_without_home_links_to_home_list()
    {
        SetCurrentHome(null);

        var cut = Render<BottomNavigation>();

        var links = cut.FindAll("a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal(["/homes/list", "/datasources", "/users/details"], links);
    }

    [Fact]
    public void Bottom_navigation_marks_current_section()
    {
        SetCurrentHome("h1");
        Services.GetRequiredService<NavigationManager>().NavigateTo("/datasources/details/42");

        var cut = Render<BottomNavigation>();

        Assert.Equal("/datasources", cut.Find("a.active").GetAttribute("href"));
    }
}
