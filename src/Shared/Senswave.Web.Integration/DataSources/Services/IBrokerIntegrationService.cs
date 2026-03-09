using Refit;
using Senswave.Web.Integration.DataSources.Response;

namespace Senswave.Web.Integration.DataSources.Services;

public interface IBrokerIntegrationService
{
    [Get("/api/v1/datasources/brokers/{id}/clients")]
    Task<ClientStateResponse> GetClientState([AliasAs("id")] string id);
}
