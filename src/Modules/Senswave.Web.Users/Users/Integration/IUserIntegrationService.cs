using Refit;
using Senswave.Web.Users.Users.Models;

namespace Senswave.Web.Users.Users.Integration;

public interface IUserIntegrationService
{
    [Delete("/api/v1/users/account")]
    Task DeleteAccount(CancellationToken cancellationToken = default);

    [Get("/api/v1/users")]
    Task<UserDetailsResponse> GetInfo();

    [Patch("/api/v1/users/settings")]
    Task UpdateSettings([Body] UserSettingUpdateRequest settings);

    [Post("/api/v1/users/consents")]
    Task AcceptConsents();

    [Get("/api/v1/legal/terms")]
    Task<LegalDocumentResponse> GetTermsAndConditions();

    [Get("/api/v1/legal/privacy")]
    Task<LegalDocumentResponse> GetPrivacyPolicy();
}
