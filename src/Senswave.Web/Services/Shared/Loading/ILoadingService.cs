namespace Senswave.Web.Services.Shared.Loading;

public interface ILoadingService
{
    bool Loading { get; }

    void StartLoading(string key);

    void StopLoading(string key);
}
