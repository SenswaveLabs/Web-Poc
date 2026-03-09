using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Integration.Auth.Services;
using Senswave.Web.Integration.DataSources.Services;
using Senswave.Web.Integration.Handlers;
using Senswave.Web.Integration.Homes;

namespace Senswave.Web.Integration;

public static class SenswaveRestExtensions
{
    public static IServiceCollection AddSenswaveIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRefitClient<IAuthIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!));

        services.AddTransient<AuthHeaderHandler>();

        services.AddRefitClient<IUserIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IHomesIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddRefitClient<IBrokerIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
