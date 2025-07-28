using JetBrains.Annotations;

namespace Social.Shared.Errors;

public sealed class Failure(
    Exception exception,
    [StructuredMessageTemplate]
    string message,
    params object[] args)
    : Error(message, args)
{
    public Exception Exception { get; } = exception;
}