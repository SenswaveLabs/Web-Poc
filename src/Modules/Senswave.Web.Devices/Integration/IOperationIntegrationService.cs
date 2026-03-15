using Refit;
using System.Text.Json.Nodes;
namespace Senswave.Web.Devices.Integration;

public record CreateOperationRequest(string DeviceId, string Name, string Type, Dictionary<string, JsonNode> Configuration, string Topic);
public record OperationCreatedResponse(string Id);
public record GetOperationResponse(string Id, string Topic, string Name, string Type, Dictionary<string, JsonNode> Configuration);
public record DisplayOperationsResponse(List<OperationDto> Items);
public record OperationDto(string Id, string Name, string Type);

public interface IOperationIntegrationService
{
    [Post("/api/v1/devices/operations")]
    Task<OperationCreatedResponse> CreateOperationAsync([Body] CreateOperationRequest request);

    [Get("/api/v1/devices/operations/display")]
    Task<DisplayOperationsResponse> DisplayOperationsAsync([Query] string deviceId, int page = 1, int size = 10);

    [Get("/api/v1/devices/operations/{operationId}")]
    Task<GetOperationResponse> GetOperationAsync(string operationId);

    [Delete("/api/v1/devices/operations/{operationId}")]
    Task DeleteOperationAsync(string operationId);
}
