namespace Senswave.Web.Shared.Resulting;

public record Error
{
    public static Error None => new("None");

    public static Error Unknown => new("Unknown");

    public string Code { get; }

    public string Description { get; }

    public Error(string code, string? description = null)
    {
        Code = code;
        Description = description ?? $"Error: {code}. No Description";
    }

    public override string ToString() =>
        $"Error Code: {Code}, Description: {Description}";
}
