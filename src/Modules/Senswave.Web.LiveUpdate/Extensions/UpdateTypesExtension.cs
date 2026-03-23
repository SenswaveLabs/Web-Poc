using Senswave.Web.LiveUpdate.Models;

namespace Senswave.Web.LiveUpdate.Extensions;

public static class UpdateTypesExtension
{
    public static UpdateType ToUpdateType(this string type) => type switch
    {
        "deviceTileActionUpdate" => UpdateType.DeviceTileActionUpdate,
        "widgetsActionUpdate" => UpdateType.WidgetsActionUpdate,
        "dataSourceStateUpdate" => UpdateType.DataSourceStateUpdate,
        _ => throw new NotSupportedException()
    };
}
