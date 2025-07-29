namespace Social.Compose;

public static class UserService
{
    public static IResourceBuilder<ProjectResource> AddUserService(this IDistributedApplicationBuilder builder,
        IResourceBuilder<AzureCosmosDBResource> cosmosDb,
        IResourceBuilder<IResourceWithConnectionString> messaging)
    {
        var dbConfig = builder.Configuration.GetSection("UserService:Db");
        var authConfig = builder.Configuration.GetSection("UserService:Auth");
        var serviceBusConfig = builder.Configuration.GetSection("UserService:ServiceBus");

        return builder
            .AddProject<Projects.UserService>("user-service")
            .WithEnvironment("Database", dbConfig["Database"])
            .WithEnvironment("Containers:Users:Name", dbConfig["Containers:Users:Name"])
            .WithEnvironment("Containers:Users:PartitionKeyPath", dbConfig["Containers:Users:PartitionKeyPath"])
            .WithEnvironment("Containers:UserFollows:Name", dbConfig["Containers:UserFollows:Name"])
            .WithEnvironment("Containers:UserFollows:PartitionKeyPath", dbConfig["Containers:UserFollows:PartitionKeyPath"])
            .WithEnvironment("Auth:Salt", authConfig["Salt"])
            .WithEnvironment("Auth:Issuer", authConfig["Issuer"])
            .WithEnvironment("Auth:Lifetime", authConfig["Lifetime"])
            .WithEnvironment("Auth:Key", authConfig["Key"])
            .WithEnvironment("ServiceBus:0:Topic", serviceBusConfig["Topic"])
            .WithEnvironment("ServiceBus:0:Key", serviceBusConfig["Key"])
            .WithReference(cosmosDb, "UserStorage")
            .WithReference(messaging, "Messaging")
            .WaitFor(messaging)
            .WaitFor(cosmosDb);
    }
}