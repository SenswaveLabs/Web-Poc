using Senswave.Web.Homes.Integration;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Homes.Services;

public interface IRoomService
{
    List<RoomDto> Rooms { get; }

    Task<Result> LoadRooms();
}
