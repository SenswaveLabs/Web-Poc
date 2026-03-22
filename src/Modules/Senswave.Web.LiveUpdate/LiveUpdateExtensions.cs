using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senswave.Web.Shared.Requests;

namespace Senswave.Web.LiveUpdate;

public static class LiveUpdateExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }
}
