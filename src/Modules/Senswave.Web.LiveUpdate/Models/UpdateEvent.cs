namespace Senswave.Web.LiveUpdate.Models;

public class UpdateEvent
{
    public UpdateType Type { get; set; }
    public string? Payload { get; set; }
}
