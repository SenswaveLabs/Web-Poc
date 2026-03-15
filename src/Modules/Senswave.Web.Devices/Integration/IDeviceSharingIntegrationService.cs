using Refit;

namespace Senswave.Web.Devices.Integration;


public record GetSharingsResponse(List<SharingDto> Items);
public record SharingDto(string SharingId, string FriendEmail, string SharingType);
public record SetSharingRequest(string DeviceId, string SharingType, string FriendEmail);

public interface IDeviceSharingIntegrationService
{
    [Get("/api/v1/devices/sharings")]
    Task<GetSharingsResponse> GetSharingsAsync([Query] string deviceId);

    [Put("/api/v1/devices/sharings")]
    Task SetSharingAsync([Body] SetSharingRequest request);

    [Delete("/api/v1/devices/sharings/{deviceSharingId}")]
    Task DeleteSharingAsync(string deviceSharingId);
}
