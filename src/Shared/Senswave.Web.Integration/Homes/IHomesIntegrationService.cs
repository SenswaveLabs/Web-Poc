using Refit;

namespace Senswave.Web.Integration.Homes;

public record CreateHomeRequest(string? DataSourceId, string Name, string Icon, double? Latitude, double? Longitude);
public record UpdateHomeRequest(string? DataSourceId, string Name, string Icon, double? Latitude, double? Longitude);
public record CurrentHomeResponse(string Id);
public record AssignHomeDataSourceRequest(string BrokerId);
public record LocationDto(double Longitude, double Latitude);
public record GetHomeResponse(string Id, string Name, string Icon, bool IsOwner, DataSourceDto DataSource, LocationDto Location, List<RoomDto> Rooms);
public record ListHomeDto(string Id, string? DataSourceId, string Name, string Icon, bool IsOwner, LocationDto Location);
public record DataSourceDto(string? Id, string State, string Name);

public interface IHomesIntegrationService
{
    [Get("/api/v1/homes")]
    Task<ListResponse<ListHomeDto>> GetHomes(int page = 1, int size = 10);

    [Get("/api/v1/homes/{homeId}")]
    Task<GetHomeResponse> GetHome([AliasAs("homeId")] string homeId);

    [Get("/api/v1/homes/current")]
    Task<CurrentHomeResponse> GetCurrentHome();

    [Post("/api/v1/homes/")]
    Task<CurrentHomeResponse> CreateHome([Body] CreateHomeRequest request);


    [Patch("/api/v1/homes/{homeId}")]
    Task UpdateHome([AliasAs("homeId")] string homeId, [Body] UpdateHomeRequest request);

    [Delete("/api/v1/homes/{homeId}")]
    Task DeleteHome(Guid homeId);

    [Put("/api/v1/homes/{homeId}/datasource")]
    Task SetHomeDataSourceAsync([AliasAs("homeId")] string homeId, [Body] AssignHomeDataSourceRequest request);

    [Delete("/api/v1/homes/{homeId}/datasource")]
    Task DeleteHomeDataSourceAsync(Guid homeId);
}
