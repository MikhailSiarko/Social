using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Api;

public sealed record ErrorResult(string? Message, IDictionary<string, string[]>? Errors);

public static class ResultHelper
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsError
            ? result.Error is ValidationError validationError
                ? Results.BadRequest(ToHttpResult(validationError))
                : result.Error is NotFound notFound
                    ? Results.NotFound(ToHttpResult(notFound))
                    : Results.InternalServerError(ToHttpResult(result.Error))
            : Results.Ok(result.Value);
    }

    private static ErrorResult ToHttpResult(Error error)
    {
        return error is ValidationError { Errors: { } errors }
            ? new ErrorResult(null, errors)
            : new ErrorResult(error.ToString(), null);
    }
}