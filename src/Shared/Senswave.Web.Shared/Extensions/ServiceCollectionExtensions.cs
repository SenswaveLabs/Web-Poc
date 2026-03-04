using Microsoft.Extensions.DependencyInjection;
using Senswave.Web.Shared.Resulting;

namespace Senswave.Web.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSenswaveShared(this IServiceCollection services)
    {
        services.AddSingleton<IErrorFactory, ErrorFactory>();

        return services;
    }
}
