namespace Senswave.Web.Devices.Services;

public interface IDeviceDetailsService
{
    Task<string> GetRoomNameById(string id);
}
