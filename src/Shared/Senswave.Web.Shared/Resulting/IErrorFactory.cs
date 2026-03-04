namespace Senswave.Web.Shared.Resulting;

public interface IErrorFactory
{
    Result Create(string code, string? defaultDescription = null);
    Result<T> Create<T>(string code, string? defaultDescription = null);
}