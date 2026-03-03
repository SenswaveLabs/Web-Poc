namespace Senswave.Web.Services.Shared.Loading;

public class LoadingService(ILogger<LoadingService> logger) : ILoadingService
{
    private HashSet<string> _loadingKeys = [];

    private bool _isLoading;

    public bool Loading => _isLoading;

    public void StartLoading(string key)
    {
        _isLoading = true;
        _loadingKeys.Add(key);

        logger.LogInformation("[Key: {key}] Starting loading.", key);
    }

    public void StopLoading(string key)
    {
        _isLoading = false;
        _loadingKeys.Remove(key); 
        logger.LogInformation("[Key: {key}] Stopping loading.", key);

        if (_loadingKeys.Count == 0)
        {
            _isLoading = false;
            logger.LogInformation("[Key: {key}] Loading stopped.", key);
        }
    }
}
