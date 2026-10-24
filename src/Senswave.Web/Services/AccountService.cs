using Refit;
using Senswave.Web.Shared.Resulting;
using Senswave.Web.Users.Auth.Integration;
using Senswave.Web.Users.Auth.Models;
using Senswave.Web.Users.Auth.Services;

namespace Senswave.Web.Services;

public class AccountService(
    IAuthIntegrationService authIntegrationService,
    ILogger<AccountService> logger) : IAccountService
{
    public Task<Result> Register(string email, string password) =>
        Send("register", () => authIntegrationService.Register(new RegisterRequest
        {
            Email = email,
            Password = password
        }));

    public Task<Result> ResendConfirmationEmail(string email) =>
        Send("resend confirmation email", () => authIntegrationService.ResendConfirmationEmail(new EmailRequest
        {
            Email = email
        }));

    public Task<Result> ForgotPassword(string email) =>
        Send("request password reset", () => authIntegrationService.ForgotPassword(new EmailRequest
        {
            Email = email
        }));

    public Task<Result> ResetPassword(string email, string resetCode, string newPassword) =>
        Send("reset password", () => authIntegrationService.ResetPassword(new ResetPasswordRequest
        {
            Email = email,
            ResetCode = resetCode.Trim(),
            NewPassword = newPassword
        }));

    private async Task<Result> Send(string operation, Func<Task> request)
    {
        try
        {
            await request();
            return Result.Success();
        }
        catch (ApiException ex)
        {
            var errors = await IdentityProblemDetails.ToErrors(ex);

            logger.LogWarning("Failed to {Operation}. Status: {StatusCode}. Errors: {Errors}",
                operation, ex.StatusCode, string.Join(", ", errors.Select(x => x.Code)));

            return Result.Failure(errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to {Operation}.", operation);
            return Result.Failure(new Error(AuthErrorCodes.Unknown));
        }
    }
}
