using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Social.Shared.Errors;

public partial class Error([StructuredMessageTemplate] string? message, params object[] args)
{
    public string? Message { get; } = message;
    public object[] Args { get; } = args;

    public override string ToString() =>
        !string.IsNullOrEmpty(Message)
            ? string.Format(ConvertToCompositeFormat(Message), Args)
            : string.Empty;

    private static string ConvertToCompositeFormat(string template)
    {
        var index = 0;
        return MyRegex().Replace(template, _ => $"{{{index++}}}");
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex MyRegex();
}