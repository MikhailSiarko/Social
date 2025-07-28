using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;
using Social.Services.User.Application.Models;

namespace Social.Services.User.Api.Routes;

public static class Login
{
    public static WebApplication MapLogin(this WebApplication app)
    {
        app.MapPost("/login",
            [AllowAnonymous] static async ([FromBody] LoginUserModel model, [FromServices] IApplicationService appService,
                CancellationToken token) =>
            {
                var authResult = await appService.LoginUserAsync(model.Email, model.Password, token);
                return authResult.ToHttpResult();
            }
        ).WithName("Login");

        return app;
    }
}
