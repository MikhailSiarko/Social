using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;
using Social.Services.User.Application.Models;

namespace Social.Services.User.Api.Routes;

public static class User
{
    public static WebApplication MapUser(this WebApplication app)
    {
        app.MapPatch("/",
                [Authorize] static async ([FromBody] PatchUserModel model,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var authResult = await appService.UpdateUserAsync(model, token);
                    return authResult.ToHttpResult();
                }
            )
            .WithName("Update");

        return app;
    }
}