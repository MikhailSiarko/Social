using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Social.Infrastructure.DependencyInjection;
using Social.Services.User.Domain;
using Social.Services.User.Persistence;

namespace Social.Services.User.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddTransient<IApplicationService, ApplicationService>()
            .AddServiceBus(configuration)
            .AddDomain()
            .AddPersistence();
    }
}
