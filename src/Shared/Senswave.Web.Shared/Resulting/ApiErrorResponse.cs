namespace Senswave.Web.Shared.Resulting;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<ApiError> Errors { get; set; } = [];
    public string TraceId { get; set; } = string.Empty;
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public int Type { get; set; }
    public string Description { get; set; } = string.Empty;
}