using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Infrastructure.Communication.Abstractions;
using Social.Infrastructure.DependencyInjection;
using Social.Services.Monitoring;
using Social.Services.Search.Application;
using Social.Services.Search.Application.MessageHandlers;
using Social.Services.Search.Persistence;
using Social.Services.Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddServiceDefaults();
builder.Services.AddServiceBus(builder.Configuration);
builder.Services.AddTransient<IMessageHandler<UserCreated>, UserCreatedMessageHandler>();
builder.Services.AddTransient<IMessageHandler<UserUpdated>, UserUpdatedMessageHandler>();
builder.Services.AddSingleton<ISearchRepository, SearchRepository>();
builder.Services.AddHostedService<UserMessageListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/users",
    [AllowAnonymous] async ([FromQuery(Name = "q")] string query, [FromServices] ISearchRepository repository,
        CancellationToken token) =>
    {
        var result = await repository.SearchAsync(query, token);
        if (result.IsError)
            Results.InternalServerError();
        return Results.Ok(result.Value);
    });

app.Run();