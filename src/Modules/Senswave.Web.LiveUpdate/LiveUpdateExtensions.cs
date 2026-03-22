using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senswave.Web.LiveUpdate.Services;

namespace Senswave.Web.LiveUpdate;

public static class LiveUpdateExtensions
{
    public static IServiceCollection AddLiveUpdates(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ILiveUpdateService, LiveUpdateService>();

        return services;
    }
}
