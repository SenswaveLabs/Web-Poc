using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Senswave.Web.Users;

public static class UsersExtensions
{
    public static IServiceCollection AddUsers(this IServiceCollection services, IConfiguration configuration)
    {


        return services;
    }
}
