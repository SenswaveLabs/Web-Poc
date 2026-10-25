using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Senswave.Web.Homes.Integration;
using Senswave.Web.Services;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Tests.Services;

public class HomeServiceTests
{
    private sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(Anonymous);

        public void SignOut() => NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private readonly TestAuthenticationStateProvider _auth = new();
    private readonly IHomesIntegrationService _homes = Substitute.For<IHomesIntegrationService>();
    private readonly HomeService _service;

    public HomeServiceTests()
    {
        _service = new HomeService(
            _auth,
            Substitute.For<IErrorFactory>(),
            Substitute.For<IRoomsIntegrationServcice>(),
            _homes,
            Substitute.For<IHomesSharingIntegrationService>(),
            NullLogger<HomeService>.Instance);
    }

    private void CurrentHomeIs(string id, string name)
    {
        _homes.GetCurrentHome().Returns(new CurrentHomeResponse(id));
        _homes.GetHome(id).Returns(new GetHomeResponse(id, name, "home", true, null!, null!, []));
    }

    [Fact]
    public async Task Next_user_gets_own_home_after_previous_user_signs_out()
    {
        CurrentHomeIs("h-alice", "Alice's flat");
        await _service.Initialize();
        Assert.Equal("h-alice", _service.CurrentHome?.Id);

        var changes = 0;
        _service.OnChange += () => changes++;
        _auth.SignOut();

        Assert.Null(_service.CurrentHome);
        Assert.Equal(1, changes);

        CurrentHomeIs("h-bob", "Bob's house");
        await _service.Initialize();
        Assert.Equal("Bob's house", _service.CurrentHome?.Name);
    }

    [Fact]
    public void Dispose_unsubscribes_from_authentication_changes()
    {
        var changes = 0;
        _service.OnChange += () => changes++;

        _service.Dispose();
        _auth.SignOut();

        Assert.Equal(0, changes);
    }
}
