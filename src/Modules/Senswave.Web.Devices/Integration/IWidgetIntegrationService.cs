using Refit;
using System.Text.Json.Nodes;

namespace Senswave.Web.Devices.Integration;

public record CreateWidgetRequest(string OperationId, string Name, string Type, Dictionary<string, JsonNode> Configuration);
public record WidgetCreatedResponse(string Id);
public record GetWidgetResponse(string Id, string DeviceId, string OperationId, string Name, string Type, bool Enabled, JsonObject Configuration);
public record DisplayWidgetsResponse(List<DisplayGroupDto> Items);
public record DisplayGroupDto(DisplayOperationDto Operation, List<WidgetDto> Widgets);
public record DisplayOperationDto(string Id, string Name, string Type);
public record WidgetDto(string Id, string Name, string Type, bool Enabled);
public record StateRequest(bool Enabled);
public record WidgetActionRequest(JsonValue Value);

public interface IWidgetIntegrationService
{
    [Post("/api/v1/devices/widgets")]
    Task<WidgetCreatedResponse> CreateWidgetAsync([Body] CreateWidgetRequest request);

    [Get("/api/v1/devices/widgets/display")]
    Task<DisplayWidgetsResponse> DisplayWidgetsAsync([Query] string deviceId);

    [Get("/api/v1/devices/widgets/{widgetId}")]
    Task<GetWidgetResponse> GetWidgetAsync(string widgetId);

    [Delete("/api/v1/devices/widgets/{widgetId}")]
    Task DeleteWidgetAsync(string widgetId);

    [Put("/api/v1/devices/widgets/{widgetId}/state")]
    Task SetWidgetStateAsync(string widgetId, [Body] StateRequest request);

    [Post("/api/v1/devices/widgets/{widgetId}/action")]
    Task ExecuteWidgetActionAsync(string widgetId, [Body] WidgetActionRequest request);
}
