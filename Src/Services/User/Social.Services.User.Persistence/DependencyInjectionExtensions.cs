using Microsoft.Extensions.DependencyInjection;
using Social.Services.User.Domain.Persistence;

namespace Social.Services.User.Persistence;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserFollowRepository, UserFollowRepository>();
        return services;
    }
}