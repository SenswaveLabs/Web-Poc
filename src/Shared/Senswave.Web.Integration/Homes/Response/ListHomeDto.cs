namespace Senswave.Web.Integration.Homes.Response;

public class ListHomeDto
{
    public string Id { get; set; } = string.Empty;

    public string DataSourceId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool IsOwner { get; set; } = true;
}
