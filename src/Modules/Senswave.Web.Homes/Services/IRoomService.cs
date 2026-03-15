using Senswave.Web.Integration.Homes;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Homes.Services;

public interface IRoomService
{
    Task<Result<List<RoomDto>>> GetRooms();
}
