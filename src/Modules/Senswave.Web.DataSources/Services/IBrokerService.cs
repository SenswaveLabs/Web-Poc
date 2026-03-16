using Senswave.Web.DataSources.Integration;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.DataSources.Services;

public interface IBrokerService
{
    Task<Result> AddBroker(CreateBrokerModel model);

    Task<Result> UpdateBroker(string id, string name);

    Task<Result> DeleteBroker(string id);

    Task<Result<BrokerModel>> GetBroker(string id);

    Task<Result<List<BrokerDto>>> GetBrokers();

    Task<Result> StartClient(string id, string username, string password);

    Task<Result> StopClient(string id);

    Task<Result> RestartClient(string id);
}
