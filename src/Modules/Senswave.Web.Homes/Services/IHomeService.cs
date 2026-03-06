using Senswave.Web.Homes.Models;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Homes.Services;

public interface IHomeService
{
    HomeDetails? CurrentHome { get; }

    event Action? OnChange;

    Task<Result> Initialize();

    Task<Result<List<Home>>> GetHomes();
}
