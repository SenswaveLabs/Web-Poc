namespace Senswave.Web.Integration;

public class ListResponse<T> where T : class
{
    public List<T> Items { get; set; } = [];
}
