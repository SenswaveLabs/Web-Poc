namespace Senswave.Web.Users.Users.Models;

public class LegalDocumentResponse
{
    public Guid Id { get; set; }

    public string Version { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
