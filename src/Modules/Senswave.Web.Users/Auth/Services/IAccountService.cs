using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Users.Auth.Services;

public interface IAccountService
{
    Task<Result> Register(string email, string password);

    Task<Result> ResendConfirmationEmail(string email);

    Task<Result> ForgotPassword(string email);

    Task<Result> ResetPassword(string email, string resetCode, string newPassword);
}
