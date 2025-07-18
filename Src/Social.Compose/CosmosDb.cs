namespace Social.Compose;

public static class CosmosDb
{
    public static IResourceBuilder<AzureCosmosDBResource> AddCosmosDb(this IDistributedApplicationBuilder builder)
    {
        return builder
            .AddAzureCosmosDB("azure-cosmos-db")
            .RunAsEmulator(x =>
                {
                    x.WithContainerName("social.cosmos.db")
                        .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
                        .WithContainerRuntimeArgs("-p", "8081:8081", "-p", "10250-10255:10250-10255")
                        .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "2")
                        .WithUrl("https://localhost:8081/_explorer/index.html", "Data Explorer")
                        .WithOtlpExporter();
                }
            );
    }
}