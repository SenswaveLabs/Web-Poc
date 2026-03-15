using Senswave.Web.Shared.Resulting;
using Senswave.Web.Users.Users.Models;

namespace Senswave.Web.Users.Users.Services;

public interface IUserService
{
    UserDetails Details { get; }

    Task<Result> Initialize();

    Task<Result> RemoveAccount();

    Task<Result> OverrideSettings(string? theme = null, string? language = null);
}
