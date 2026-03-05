namespace Senswave.Web.Shared.Services;

public interface ILoadingService
{
    bool Loading { get; }

    Action? OnChange { get; set; }

    void Show(string key);

    void Hide(string key);
}
