using Senswave.Web.Devices.Integration;
using Senswave.Web.Devices.Models;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Devices.Services;

public interface IDeviceDetailsService
{
    Task<string> GetRoomNameById(string id);

    Task<Result<DeviceModel>> GetDevice(string id);

    Task<Result<List<DisplayGroupDto>>> GetWidgetsAndOperations(string id);

    Task<Result<List<DetailedDashboardDto>>> GetDashboards(string id);

    Task<Result> CreateDevice(DeviceModel dto);

    Task<Result> UpdateDevice(DeviceModel dto);

    Task<Result> DeleteDevice(string id);
}
