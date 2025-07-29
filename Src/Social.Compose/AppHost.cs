using Social.Compose;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafkaServer();
var cosmosDb = builder.AddCosmosDb();
builder.AddUserService(cosmosDb, messaging);

builder.Build().Run();