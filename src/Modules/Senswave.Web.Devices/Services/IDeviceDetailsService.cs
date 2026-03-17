using Senswave.Web.Devices.Models;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Devices.Services;

public interface IDeviceDetailsService
{
    Task<string> GetRoomNameById(string id);

    Task<Result> CreateDevice(DeviceModel dto);

    Task<Result> UpdateDevice(DeviceModel dto);
}
