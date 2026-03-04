namespace Senswave.Web.Shared.Resulting;

public class ErrorFactory : IErrorFactory
{
    public Result Create(string code, string? defaultDescription = null) => Result.Failure(new Error(code, defaultDescription));
    public Result<T> Create<T>(string code, string? defaultDescription = null)  => Result<T>.Failure(new Error(code, defaultDescription));
}
