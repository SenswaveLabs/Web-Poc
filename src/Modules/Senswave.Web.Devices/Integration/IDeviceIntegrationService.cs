using Refit;
using System.Text.Json.Nodes;

namespace Senswave.Web.Devices.Integration;

public record CreateDeviceRequest(string HomeId, string RoomId, string Name, string Icon);
public record DeviceCreatedResponse(string Id);
public record DeviceDto(string Id, string Name, string Icon, string RoomId, DeviceTileDto Tile);
public record DeviceTileDto(string Type, string OperationId);
public record DisplayDevicesResponse(List<DisplayDeviceDto> Items);
public class DisplayDeviceDto
{
    public string Id { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DisplayDeviceTileDto? Tile { get; set; } 
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DisplayDeviceTileDto
{
    public string Type { get; set; } = string.Empty;
    public JsonNode? Value { get; set; }
}

public record UpdateDeviceRequest(string? RoomId, string Name, string Icon, string? OperationId, string? Type);
public record DeviceTileActionRequest(JsonValue Value);

public interface IDeviceIntegrationService
{
    [Post("/api/v1/devices")]
    Task<DeviceCreatedResponse> CreateDeviceAsync([Body] CreateDeviceRequest request);

    [Get("/api/v1/devices/display")]
    Task<DisplayDevicesResponse> DisplayDevicesAsync([Query] string homeId, int? page, int? size);

    [Get("/api/v1/devices/{deviceId}")]
    Task<DeviceDto> GetDeviceAsync(string deviceId);

    [Patch("/api/v1/devices/{deviceId}")]
    Task UpdateDeviceAsync(string deviceId, [Body] JsonObject request);

    [Delete("/api/v1/devices/{deviceId}")]
    Task DeleteDeviceAsync(string deviceId);

    [Get("/api/v1/devices/{deviceId}/display")]
    Task<DisplayDeviceDto> GetDeviceDisplayAsync(string deviceId);

    [Post("/api/v1/devices/{deviceId}/tile/action")]
    Task<DisplayDeviceDto> ExecuteDeviceTileActionAsync(string deviceId, [Body] DeviceTileActionRequest request);
}
