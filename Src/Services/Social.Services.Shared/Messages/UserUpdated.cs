using Social.Infrastructure.Communication.Abstractions;

namespace Social.Services.Shared.Messages;

public sealed class UserUpdated(Guid userId, string? userName, string? firstName, string? lastName) : Message
{
    public Guid UserId { get; set; } = userId;
    public string? UserName { get; set; } = userName;
    public string? FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public override Guid CorrelationId { get; protected set; } = userId;
}