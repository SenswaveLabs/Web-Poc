using Senswave.Web.Homes.Models;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Homes.Services;

public interface IHomeService
{
    HomeDetails? CurrentHome { get; }

    event Action? OnChange;

    Task<Result> Initialize();

    Task<Result<List<Home>>> GetHomes();

    Task<Result> ChangeHome(string newHomeId);

    Task<Result> CreateHome(string name, string icon, double? lattitude, double? longitude);

    Task<Result> JoinHome(string code);

    Task<Result> UpdateHomeDataSources(string dataSourceId);

    Task<Result> UpdateHome(string name, string icon, double? lattitude, double? longitude);

    Task<Result> RefreshCurrentHome();
}
