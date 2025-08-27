using System.Diagnostics.CodeAnalysis;
using Social.Shared.Errors;

namespace Social.Shared;

public readonly struct Result<TOk>
{
    public readonly TOk? Value;
    public readonly Error? Error;

    private Result(TOk value)
    {
        Value = value;
        IsOk = true;
    }

    private Result(Error error)
    {
        Error = error;
        IsError = true;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsOk { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError { get; }

    public static implicit operator Result<TOk>(TOk value) => FromValue(value);
    public static implicit operator Result<TOk>(Error error) => FromError(error);

    public static Result<TOk> FromValue(TOk value) => new(value);
    public static Result<TOk> FromError(Error error) => new(error);
}