using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Social.Shared.Errors;

namespace Social.Shared;

public static class LoggerExtensions
{
    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public static void LogError<T>(this ILogger<T> logger, Error error)
    {
        switch (error)
        {
            case ValidationError { Errors: { } errors }:
                foreach (var (key, value) in errors)
                    logger.LogError("{Key}: {Errors}", key, string.Join(Environment.NewLine, value));
                break;
            case Failure failure:
                logger.LogError(failure.Exception, failure.Message, failure.Args);
                break;
            default:
                logger.LogError(error.Message, error.Args);
                break;
        }
    }
}