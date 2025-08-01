namespace Social.Compose;

public static class SearchService
{
    public static IResourceBuilder<ProjectResource> AddSearchService(this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> messaging)
    {
        var serviceBusConfig = builder.Configuration.GetSection("SearchService:ServiceBus");
        var kafkaConfig = builder.Configuration.GetSection("UserService:Kafka");

        var elastic = builder
            .AddElasticsearch("elastic-search")
            .WithContainerName("elastic.search")
            .WithImageTag("9.1.0")
            .WithOtlpExporter();

        return builder
            .AddProject<Projects.SearchService>("search-service")
            .WithEnvironment("ServiceBus:0:Topic", serviceBusConfig["Topic"])
            .WithEnvironment("ServiceBus:0:Key", serviceBusConfig["Key"])
            .WithEnvironment("Kafka:Partitions", kafkaConfig["Partitions"])
            .WithEnvironment("ElasticSearch:UserIndex", builder.Configuration["ElasticSearch:UserIndex"])
            .WithReference(messaging, "Messaging")
            .WithReference(elastic, "ElasticSearch:Url")
            .WaitFor(messaging)
            .WaitFor(elastic);
    }
}