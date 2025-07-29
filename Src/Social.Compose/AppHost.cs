using Social.Compose;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafkaServer();
var userMongoDb = builder.AddMongoDb("user");
builder.AddUserService(userMongoDb, messaging);

builder.Build().Run();