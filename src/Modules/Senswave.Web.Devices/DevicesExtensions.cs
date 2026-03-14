using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Integration.Handlers;

namespace Senswave.Web.Devices;

public static class DevicesExtensions
{
    public static IServiceCollection AddDevices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthHeaderHandler>();

        services.AddRefitClient<IDeviceIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
