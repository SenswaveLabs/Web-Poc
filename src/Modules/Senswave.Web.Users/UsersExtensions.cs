using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Senswave.Web.Shared.RestServices;

namespace Senswave.Web.Users;

public static class UsersExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddRefitClient<IAuthRestService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:5001"));
        return services;
    }
}
