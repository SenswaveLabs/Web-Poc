using Refit;

namespace Senswave.Web.Integration.Homes;

public record CreateRoomRequest(string Name);
public record UpdateRoomRequest(string Name);
public record GetRoomResponse(string Id, string Name);
public record RoomDto(string Id, string Name);
public record DisplayRoomsResponse(List<RoomDto> Items);

public interface IRoomsIntegrationServcice
{
    [Get("/api/v1/homes/{homeId}/rooms")]
    Task<DisplayRoomsResponse> GetRoomsAsync(string homeId);

    [Post("/api/v1/homes/{homeId}/rooms")]
    Task CreateRoomAsync(string homeId, [Body] CreateRoomRequest request);

    [Get("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task<GetRoomResponse> GetRoomAsync(string homeId, string roomId);

    [Patch("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task UpdateRoomAsync(string roomId, [Body] UpdateRoomRequest request);

    [Delete("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task DeleteRoomAsync(string homeId, string roomId);

    [Get("/api/v1/homes/{homeId}/rooms/display")]
    Task<DisplayRoomsResponse> DisplayRoomsAsync(string homeId);
}
