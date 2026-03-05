using Senswave.Web.Shared.Services;

namespace Senswave.Web.Services.Shared;

public class LoadingService(ILogger<LoadingService> logger) : ILoadingService
{

    private HashSet<string> _loadingKeys = [];

    private bool _isLoading;

    public bool Loading => _isLoading;
    
    public Action? OnChange { get; set; }

    public void Show(string key)
    {
        _isLoading = true;
        _loadingKeys.Add(key);

        OnChange?.Invoke();

        logger.LogInformation("[Key: {key}] Starting loading.", key);
    }

    public void Hide(string key)
    {
        _isLoading = false;
        _loadingKeys.Remove(key); 
        logger.LogInformation("[Key: {key}] Stopping loading.", key);

        if (_loadingKeys.Count == 0)
        {
            OnChange?.Invoke();
            _isLoading = false;
            logger.LogInformation("[Key: {key}] Loading stopped.", key);
        }
    }
}
