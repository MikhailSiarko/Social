using JetBrains.Annotations;

namespace Social.Shared.Errors;

public sealed class NotFound([StructuredMessageTemplate] string message, params object[] args)
    : Error(message, args);