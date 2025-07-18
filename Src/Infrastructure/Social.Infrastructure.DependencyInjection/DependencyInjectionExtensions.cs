using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Infrastructure.Communication;
using Social.Infrastructure.Communication.Abstractions;

namespace Social.Infrastructure.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddServiceBus(
        this IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusOptions = configuration.GetSection("ServiceBus").Get<ServiceBusOptions[]>();
        if (serviceBusOptions is not { Length: > 0 })
            return services;

        foreach (var serviceBusOption in serviceBusOptions)
        {
            services.AddKeyedSingleton<IServiceBus, AzureServiceBus>(serviceBusOption.Key,
                (sp, _) =>
                {
                    var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                    var config = sp.GetRequiredService<IConfiguration>();
                    var env = sp.GetRequiredService<IHostEnvironment>();
                    return new AzureServiceBus(serviceBusOption, config, env, scopeFactory);
                });
        }

        return services;
    }
}