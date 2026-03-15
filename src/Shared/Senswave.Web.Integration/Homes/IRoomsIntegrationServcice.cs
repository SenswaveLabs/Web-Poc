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
    Task<DisplayRoomsResponse> GetRoomsAsync([AliasAs("homeId")] string homeId);

    [Post("/api/v1/homes/{homeId}/rooms")]
    Task CreateRoomAsync([AliasAs("homeId")] string homeId, [Body] CreateRoomRequest request);

    [Get("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task<GetRoomResponse> GetRoomAsync([AliasAs("homeId")] string homeId, [AliasAs("roomId")] string roomId);

    [Patch("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task UpdateRoomAsync(
        [AliasAs("homeId")] string homeId,
        [AliasAs("roomId")] string roomId, [Body] UpdateRoomRequest request);

    [Delete("/api/v1/homes/{homeId}/rooms/{roomId}")]
    Task DeleteRoomAsync([AliasAs("homeId")] string homeId, [AliasAs("roomId")] string roomId);

    [Get("/api/v1/homes/{homeId}/rooms/display")]
    Task<DisplayRoomsResponse> DisplayRoomsAsync([AliasAs("homeId")] string homeId);
}
