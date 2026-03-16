namespace Senswave.Web.Shared.Requests;

public class ListResponse<T> where T : class
{
    public List<T> Items { get; set; } = [];
}
