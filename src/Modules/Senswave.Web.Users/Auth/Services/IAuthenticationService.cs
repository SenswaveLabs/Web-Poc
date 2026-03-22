using Senswave.Web.Shared.Resulting;
using Senswave.Web.Users.Auth.Models;

namespace Senswave.Web.Users.Auth.Services;

public interface IAuthenticationService
{
    Task<Result> Login(LoginWithPasswordModel model);

    Task<Result> Login(string code);

    Task<Result> Logout();
}
