using Social.Services.Monitoring;
using Social.Services.User.Api.Routes;
using Social.Services.User.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddLogging();
builder.AddServiceDefaults();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapRegister();
app.MapLogin();
app.MapFollow();
app.MapUser();

app.Run();