using Aspire.Hosting.Azure;

namespace Social.Compose;

public static class AzureServiceBus
{
    public static IResourceBuilder<AzureServiceBusResource> AddServiceBus(this IDistributedApplicationBuilder builder)
    {
        var serviceBus = builder
            .AddAzureServiceBus("azure-service-bus")
            .RunAsEmulator(x =>
            {
                x.WithContainerName("social.service.bus")
                    .WithConfigurationFile("Config.json")
                    .WithOtlpExporter();
            });
        return serviceBus;
    }
}