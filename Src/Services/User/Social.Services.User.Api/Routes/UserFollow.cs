using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social.Services.User.Application;

namespace Social.Services.User.Api.Routes;

public static class UserFollow
{
    public static WebApplication MapFollow(this WebApplication app)
    {
        app.MapPost("/{userId:guid}/follow/{targetUserId:guid}",
                [AllowAnonymous] static async ([FromRoute] Guid userId, [FromRoute] Guid targetUserId,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var authResult = await appService.FollowUserAsync(userId, targetUserId, token);
                    return authResult.ToHttpResult();
                }
            )
            .WithName("Follow");

        app.MapPost("/{userId:guid}/unfollow/{targetUserId:guid}",
                [AllowAnonymous] static async ([FromRoute] Guid userId, [FromRoute] Guid targetUserId,
                    [FromServices] IApplicationService appService,
                    CancellationToken token) =>
                {
                    var unfollowResult = await appService.UnfollowUserAsync(userId, targetUserId, token);
                    return unfollowResult.ToHttpResult();
                }
            )
            .WithName("Unfollow");

        return app;
    }
}