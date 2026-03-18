using Refit;
using System.Text.Json.Nodes;

namespace Senswave.Web.Devices.Integration;

public record CreateDashboardRequest(string DeviceId, string Name, string Icon, Dictionary<string, JsonNode> Configuration);
public record UpdateDashboardRequest(string Name, string Icon);

public record DetailedDashboardDto(string Id, string Name, string Icon, string Type, int Rows, int Columns);

public record DashboardDto(string Id, string Name, string Icon, string Type);
public record DisplayDashboardsResponse(List<DashboardDto> Items);
public record GetDashboardResponse(string Id, string Name, string Icon, string Type, Dictionary<string, JsonNode> Configuration);
public record DisplayDashboardResponse(string Type, Dictionary<string, JsonNode> Configuration);
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
