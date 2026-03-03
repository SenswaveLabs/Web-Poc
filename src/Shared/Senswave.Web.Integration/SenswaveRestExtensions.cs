using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Integration.Auth.Services;
using Senswave.Web.Integration.Handlers;

namespace Senswave.Web.Integration;

public static class SenswaveRestExtensions
{
    public static IServiceCollection AddSenswaveIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRefitClient<IAuthIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!));

        services.AddRefitClient<IUserIntegrationService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Api:BaseUrl"]!))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        return services;
    }
}
