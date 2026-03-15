using Refit;
using Senswave.Web.Integration.Users.Request;
using Senswave.Web.Integration.Users.Response;

namespace Senswave.Web.Integration.Users;

public interface IUserIntegrationService
{
    [Delete("/api/v1/users/account")]
    Task DeleteAccount(CancellationToken cancellationToken = default);

    [Get("/api/v1/users")]
    Task<UserDetailsResponse> GetInfo();

    [Patch("/api/v1/users/settings")]
    Task UpdateSettings([Body] UserSettingUpdateRequest settings);
}
