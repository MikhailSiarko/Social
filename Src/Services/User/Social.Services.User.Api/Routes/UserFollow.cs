using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;
using Social.Services.User.Application.Models;

namespace Social.Services.User.Api.Routes;

public static class UserFollow
{
    public static WebApplication MapFollow(this WebApplication app)
    {
        app.MapPost("/follow",
                [Authorize] static async ([FromBody] UserFollowModel model,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var authResult = await appService.FollowUserAsync(model.UserId, token);
                    return authResult.ToHttpResult();
                }
            )
            .WithName("Follow");

        app.MapPost("/unfollow",
                [Authorize] static async ([FromBody] UserFollowModel model,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var authResult = await appService.UnfollowUserAsync(model.UserId, token);
                    return authResult.ToHttpResult();
                }
            )
            .WithName("Unfollow");

        return app;
    }
}