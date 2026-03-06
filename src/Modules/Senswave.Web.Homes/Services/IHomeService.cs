using Senswave.Web.Homes.Models;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Homes.Services;

public interface IHomeService
{
    Task<Result<List<Home>>> GetHomes();

    Task<Result<HomeDetails>> GetHome(string id);
}
