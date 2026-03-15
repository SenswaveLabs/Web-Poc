using Senswave.Web.Devices.Integration;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Devices.Services;

public interface IDeviceListService
{
    Task<Result<List<DisplayDeviceDto>>> GetListDevicesForHome();
}
