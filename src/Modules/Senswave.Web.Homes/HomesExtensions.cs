using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Homes.Integration;
using Senswave.Web.Shared.Requests;

namespace Senswave.Web.Homes;

public static class HomesExtensions
{
    public static IServiceCollection AddHomes(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRefitClient<IRoomsIntegrationServcice>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IHomesIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IHomesSharingIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
