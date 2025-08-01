using Social.Compose;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafkaServer();
var userMongoServer = builder.AddMongoServer("user");
builder.AddUserService(userMongoServer, messaging);
builder.AddSearchService(messaging);

builder.Build().Run();