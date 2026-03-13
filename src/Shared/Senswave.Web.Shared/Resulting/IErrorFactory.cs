using Refit;

namespace Senswave.Web.Shared.Resulting;

public interface IErrorFactory
{
    Result Create(string code, string? defaultDescription = null);
    Result<T> Create<T>(string code, string? defaultDescription = null);

    Task<Result> FromApiExceptionAsync(ApiException exception, string fallbackCode);
    Task<Result<T>> FromApiExceptionAsync<T>(ApiException exception, string fallbackCode);
}