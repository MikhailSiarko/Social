namespace Social.Compose;

public static class UserService
{
    public static IResourceBuilder<ProjectResource> AddUserService(this IDistributedApplicationBuilder builder,
        IResourceBuilder<MongoDBServerResource> mongoDb,
        IResourceBuilder<IResourceWithConnectionString> messaging)
    {
        var dbConfig = builder.Configuration.GetSection("UserService:Db");
        var authConfig = builder.Configuration.GetSection("UserService:Auth");
        var serviceBusConfig = builder.Configuration.GetSection("UserService:ServiceBus");
        var kafkaConfig = builder.Configuration.GetSection("UserService:Kafka");

        var userServiceDb = mongoDb
            .AddDatabase("database", dbConfig["Database"]!);

        return builder
            .AddProject<Projects.UserService>("user-service")
            .WithEnvironment("Database", dbConfig["Database"])
            .WithEnvironment("Collections:Users", dbConfig["Collections:Users"])
            .WithEnvironment("Collections:UserFollows", dbConfig["Collections:UserFollows"])
            .WithEnvironment("Auth:Salt", authConfig["Salt"])
            .WithEnvironment("Auth:Issuer", authConfig["Issuer"])
            .WithEnvironment("Auth:Lifetime", authConfig["Lifetime"])
            .WithEnvironment("Auth:Key", authConfig["Key"])
            .WithEnvironment("ServiceBus:0:Topic", serviceBusConfig["Topic"])
            .WithEnvironment("ServiceBus:0:Key", serviceBusConfig["Key"])
            .WithEnvironment("Kafka:Partitions", kafkaConfig["Partitions"])
            .WithReference(mongoDb, "UserStorage")
            .WithReference(messaging, "Messaging")
            .WaitFor(messaging)
            .WaitFor(userServiceDb);
    }
}