using Refit;

namespace Senswave.Web.Integration.Auth.Services;

public interface IUserIntegrationService
{
    [Delete("/api/v1/users/account")]
    Task DeleteAccount(CancellationToken cancellationToken = default);
}
