using System;
using System.Collections.Generic;
using System.Text;

namespace Senswave.Web.Devices.Models;

public class DeviceModel
{
    public string Id { get; set; } = string.Empty;
    public string HomeId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "bulb-outline";

    // Tile properties
    public string TileType { get; set; } = "Default";
    public string? TileOperationId { get; set; } = null;
}
