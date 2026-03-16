using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Devices.Integration;
using Senswave.Web.Shared.Requests;

namespace Senswave.Web.Devices;

public static class DevicesExtensions
{
    public static IServiceCollection AddDevices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthHeaderHandler>();

        services.AddRefitClient<IDeviceIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IDashboardIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IDeviceSharingIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IOperationIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IWidgetIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
