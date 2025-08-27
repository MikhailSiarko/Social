using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;
using Social.Services.User.Application.Models;

namespace Social.Services.User.Api.Routes;

public static class User
{
    public static WebApplication MapUser(this WebApplication app)
    {
        app.MapPost("/",
                [AllowAnonymous] static async ([FromBody] CreateUserModel model,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var userResult = await appService.CreateUserAsync(model.Email, token);
                    return userResult.ToHttpResult();
                }
            )
            .WithName("Create");

        app.MapGet("/{id:guid}",
            async (Guid id, [FromServices] IApplicationService appService, CancellationToken token) =>
            {
                var userResult = await appService.GetUserAsync(id, token);
                return userResult.ToHttpResult();
            }).WithName("Get");

        app.MapPatch("/{id:guid}",
                [AllowAnonymous] static async ([FromRoute] Guid id, [FromBody] PatchUserModel model,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var authResult = await appService.UpdateUserAsync(id, model, token);
                    return authResult.ToHttpResult();
                }
            )
            .WithName("Update");

        return app;
    }
}