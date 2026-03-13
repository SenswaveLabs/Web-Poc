using Refit;

namespace Senswave.Web.Integration.Homes;

public record CreateSharingRequest(string HomeId, string FriendEmail, string SharingType);
public record HomeSharingCreatedResponse(string InvitationId, string Password, DateTime ExpiresAtUtc, DateTime CreatedUtc);
public record AcceptSharingRequest(string Password);
public record GetShraingsResponse(List<SharingDto> Items);
public record SharingDto(string SharingId, string FriendEmail, string SharingType);


public interface IHomesSharingIntegrationService
{
    [Get("/api/v1/homes/sharings")]
    Task<GetShraingsResponse> GetSharingsAsync([Query] string homeId);

    [Post("/api/v1/homes/sharings")]
    Task<HomeSharingCreatedResponse> CreateSharingAsync([Body] CreateSharingRequest request);

    [Put("/api/v1/homes/sharings")]
    Task AcceptSharingAsync([Body] AcceptSharingRequest request);

    [Delete("/api/v1/homes/sharings/{homeSharingId}")]
    Task DeleteSharingAsync(string homeSharingId);

    [Delete("/api/v1/homes/sharings/leave/{homeId}")]
    Task LeaveHomeAsync(string homeId);
}
