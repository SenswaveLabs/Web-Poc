using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Shared.Services;

public interface ITokenStore
{
    Task<Result<string>> GetBearerToken();
}
