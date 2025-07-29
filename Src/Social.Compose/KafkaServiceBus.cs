namespace Social.Compose;

public static class KafkaServiceBus
{
    public static IResourceBuilder<KafkaServerResource> AddKafkaServer(this IDistributedApplicationBuilder builder)
    {
        var serviceBus = builder
            .AddKafka("kafka-server")
            .WithContainerName("kafka.server")
            .WithOtlpExporter();

        serviceBus.WithKafkaUI(x =>
        {
            x.WithContainerName("kafka.server.management")
                .WithParentRelationship(serviceBus);
        });

        return serviceBus;
    }
}