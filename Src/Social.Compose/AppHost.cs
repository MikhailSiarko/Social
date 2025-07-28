using Social.Compose;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddServiceBus();
var cosmosDb = builder.AddCosmosDb();
builder.AddUserService(cosmosDb, messaging);

builder.Build().Run();