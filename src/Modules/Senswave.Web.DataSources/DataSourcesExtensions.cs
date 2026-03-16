using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.DataSources.Integration;
using Senswave.Web.Shared.Requests;


namespace Senswave.Web.DataSources;

public static class DataSourcesExtensions
{
    public static IServiceCollection AddDataSources(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthHeaderHandler>();

        services.AddRefitClient<IBrokerIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
