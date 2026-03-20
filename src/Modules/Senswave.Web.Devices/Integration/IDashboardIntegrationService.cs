using Refit;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Senswave.Web.Devices.Integration;

public record CreateDashboardRequest(string DeviceId, string Name, string Icon, JsonObject Configuration);
public record UpdateDashboardRequest(string Name, string Icon);

public record DetailedDashboardDto(string Id, string Name, string Icon, string Type, int Rows, int Columns);

public record DashboardDto(string Id, string Name, string Icon, string Type);
public record DisplayDashboardsResponse(List<DashboardDto> Items);
public record GetDashboardResponse(string Id, string Name, string Icon, string Type, Dictionary<string, JsonNode> Configuration);

public record DisplayDashboardResponse(
    string Type,
    GridDashboardConfiguration Configuration
);

public record GridDashboardConfiguration(
    [property: JsonPropertyName("rows")] int Rows,
    [property: JsonPropertyName("columns")] int Columns,
    [property: JsonPropertyName("positionedWidgets")] List<PositionedWidgetDto> PositionedWidgets,
    [property: JsonPropertyName("calculatedWidgets")] List<CalculatedWidgetDto> CalculatedWidgets
);

public record PositionedWidgetDto(
    [property: JsonPropertyName("widgetId")] string WidgetId,
    [property: JsonPropertyName("row")] int Row,
    [property: JsonPropertyName("column")] int Column,
    [property: JsonPropertyName("rowSpan")] int RowSpan,
    [property: JsonPropertyName("columnSpan")] int ColumnSpan
);

public record CalculatedWidgetDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("configuration")] WidgetDetailsDto Configuration,
    [property: JsonPropertyName("updatedAtUtc")] DateTime UpdatedAtUtc
);

public record WidgetDetailsDto(
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("step")] double? Step,
    [property: JsonPropertyName("range")] WidgetRangeDto? Range,
    [property: JsonPropertyName("options")] List<JsonNode>? Options, 
    [property: JsonPropertyName("runtime")] WidgetRuntimeDto? Runtime
);

public record WidgetRangeDto(double Min, double Max);

public record WidgetRuntimeDto(
    [property: JsonPropertyName("value")] JsonNode? Value
);


public record SetWidgetOnDashboardRequest(string WidgetId, int Row, int RowSpan, int Column, int ColumnSpan);

public interface IDashboardIntegrationService
{
    [Post("/api/v1/devices/dashboards")]
    Task<CreateDashboardRequest> CreateDashboardAsync([Body] CreateDashboardRequest request);

    [Get("/api/v1/devices/dashboards/display")]
    Task<DisplayDashboardsResponse> DisplayDashboardsAsync([Query] string deviceId);

    [Get("/api/v1/devices/dashboards/{dashboardId}")]
    Task<GetDashboardResponse> GetDashboardAsync(string dashboardId);

    [Patch("/api/v1/devices/dashboards/{dashboardId}")]
    Task UpdateDashboardAsync(string dashboardId, [Body] UpdateDashboardRequest request);

    [Delete("/api/v1/devices/dashboards/{dashboardId}")]
    Task DeleteDashboardAsync(string dashboardId);

    [Get("/api/v1/devices/dashboards/{dashboardId}/display")]
    Task<DisplayDashboardResponse> GetDashboardDisplayAsync(string dashboardId);

    [Put("/api/v1/devices/dashboards/{dashboardId}/widgets")]
    Task SetWidgetOnDashboardAsync(string dashboardId, [Body] SetWidgetOnDashboardRequest request);

    [Delete("/api/v1/devices/dashboards/{dashboardId}/widgets/{widgetId}")]
    Task RemoveWidgetFromDashboardAsync(string dashboardId, string widgetId);
}
