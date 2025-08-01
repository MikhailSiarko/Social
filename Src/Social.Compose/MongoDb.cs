namespace Social.Compose;

public static class MongoDb
{
    public static IResourceBuilder<MongoDBServerResource> AddMongoServer(this IDistributedApplicationBuilder builder,
        string serviceName)
    {
        var mongodb = builder
            .AddMongoDB($"{serviceName}-mongodb-server")
            .WithContainerName($"{serviceName}.mongo.db.server")
            .WithOtlpExporter()
            .WithMongoExpress(x => x.WithContainerName("mongo.express"));

        return mongodb;
    }
}