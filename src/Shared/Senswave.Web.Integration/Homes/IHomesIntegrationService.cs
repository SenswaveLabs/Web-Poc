using Refit;
using Senswave.Web.Integration.Homes.Response;

namespace Senswave.Web.Integration.Homes;

public interface IHomesIntegrationService
{
    [Get("/api/v1/homes")]
    Task<ListResponse<ListHomeDto>> GetHomes();

    [Get("/api/v1/homes/{id}")]
    Task<HomeResponse> GetHome([AliasAs("id")] string id);

    [Get("/api/v1/homes/current")]
    Task<CurrentHomeResponse> GetCurrentHome();
}
