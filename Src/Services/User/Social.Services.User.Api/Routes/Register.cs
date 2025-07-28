using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;
using Social.Services.User.Application.Models;

namespace Social.Services.User.Api.Routes;

public static class Register
{
    public static WebApplication MapRegister(this WebApplication app)
    {
        app.MapPost("/register",
            [AllowAnonymous] async ([FromBody] RegisterUserModel model, [FromServices] IApplicationService appService,
                CancellationToken token) =>
            {
                var authResult = await appService.RegisterUserAsync(model.Email, model.Password, token);
                return authResult.ToHttpResult();
            }).WithName("Register");

        return app;
    }
}