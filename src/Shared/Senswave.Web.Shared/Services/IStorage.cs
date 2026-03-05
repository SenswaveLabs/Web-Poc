using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Shared.Services;

public interface IStorage
{
    ValueTask<Result> Set<T>(string key, T value);

    ValueTask<Result<T>> Get<T>(string key);
}
