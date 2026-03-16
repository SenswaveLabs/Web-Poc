using Refit;

namespace Senswave.Web.DataSources.Integration;


public interface IBrokerIntegrationService
{
    [Get("/api/v1/datasources/brokers")]
    Task<GetBrokersResponse> GetBrokersAsync(int page = 1, int size = 10);

    [Post("/api/v1/datasources/brokers")]
    Task<BrokerCreatedResponse> CreateBrokerAsync([Body] CreateBrokerModel request);

    [Get("/api/v1/datasources/brokers/{brokerId}")]
    Task<BrokerModel> GetBrokerAsync(string brokerId);

    [Patch("/api/v1/datasources/brokers/{brokerId}")]
    Task UpdateBrokerAsync(string brokerId, [Body] UpdateBrokerModel request);

    [Delete("/api/v1/datasources/brokers/{brokerId}")]
    Task DeleteBrokerAsync(string brokerId);

    // Data Sources - Broker Clients
    [Get("/api/v1/datasources/brokers/{brokerId}/clients")]
    Task<GetClientStateResponse> GetClientStateAsync(string brokerId);

    [Post("/api/v1/datasources/brokers/{brokerId}/clients")]
    Task StartClientAsync(string brokerId, [Body] StartClientDto request);

    [Delete("/api/v1/datasources/brokers/{brokerId}/clients")]
    Task StopClientAsync(string brokerId);

    [Patch("/api/v1/datasources/brokers/{brokerId}/clients/restart")]
    Task RestartClientAsync(string brokerId);

    // Data Sources - Broker Sessions
    [Get("/api/v1/datasources/brokers/{brokerId}/sessions")]
    Task<GetSessionsResponse> GetSessionsAsync(string brokerId, int page = 1, int size = 5);

    [Get("/api/v1/datasources/brokers/{brokerId}/sessions/{sessionId}")]
    Task<GetSessionResponse> GetSessionAsync(string brokerId, string sessionId);
}



public record CreateBrokerModel(
    string Name,
    string Url,
    string ClientName,
    int Port,
    string ProtocolVersion,
    bool UseTls,
    string Username,
    string Password);

public record UpdateBrokerModel(
    string Name,
    string Url,
    string ClientName,
    int Port,
    string ProtocolVersion,
    bool UseTls,
    string Username,
    string Password);

public record BrokerCreatedResponse(string Id);

public record BrokerModel(
    string Id,
    string Name,
    string Url,
    string ProtocolVersion,
    string ClientName,
    int Port,
    bool UseTls,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GetBrokersResponse(List<BrokerDto> Items);

public record BrokerDto(
    string Id,
    string Name,
    string Server,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record StartClientDto(string Username, string Password);

public record GetClientStateResponse(
    string ConnectionStatus,
    string LatestSessionId);

public record GetSessionsResponse(List<SessionDto> Items);

public record SessionDto(
    string Id,
    DateTime UpdatedAtUtc,
    DateTime CreatedAtUtc,
    bool Finished);

public record GetSessionResponse(
    string Id,
    List<LogDto> Logs,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record LogDto(
    string Id,
    string EventType,
    string Data,
    DateTime CreatedAtUtc);
