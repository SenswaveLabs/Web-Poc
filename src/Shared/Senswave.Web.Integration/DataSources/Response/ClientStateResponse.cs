namespace Senswave.Web.Integration.DataSources.Response;

public class ClientStateResponse
{
    public string ConnectionStatus { get; set; } = string.Empty;
    public Guid LatestSessionId { get; set; }
}
