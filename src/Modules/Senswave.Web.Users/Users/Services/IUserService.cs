
using Senswave.Web.Services.Users;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Users.Users.Services;

public interface IUserService
{
    UserDetails Details { get; }

    Task<Result> Initialize();

    Task<Result> OverrideSettings(string? theme = null, string? language = null);
}
