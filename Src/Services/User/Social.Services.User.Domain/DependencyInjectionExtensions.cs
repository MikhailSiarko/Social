using Microsoft.Extensions.DependencyInjection;
using Social.Services.User.Domain.Services;

namespace Social.Services.User.Domain;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services
            .AddTransient<IUserService, UserService>()
            .AddTransient<IUserFollowService, UserFollowService>();
    }
}