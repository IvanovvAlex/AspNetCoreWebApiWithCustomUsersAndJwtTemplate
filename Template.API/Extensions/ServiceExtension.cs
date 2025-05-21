using Template.Data.Interfaces;
using Template.Data.Repositories;
using Template.Domain.Interfaces;
using Template.Domain.Services;

namespace Template.API.Extensions;
internal static class ServiceExtension
{
    internal static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}