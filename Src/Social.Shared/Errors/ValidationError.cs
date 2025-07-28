using JetBrains.Annotations;

namespace Social.Shared.Errors;

public sealed class ValidationError : Error
{
    public ValidationError(IDictionary<string, string[]> errors) : base(null!)
    {
        Errors = errors;
    }

    public ValidationError([StructuredMessageTemplate] string message, params object[] args) :
        base(message, args)
    {
    }

    public IDictionary<string, string[]>? Errors { get; }

    public override string ToString()
    {
        return Errors != null
            ? string.Join(Environment.NewLine, Errors.Select(x => $"{x.Key}: {string.Join(", ", x.Value)}"))
            : base.ToString();
    }
}