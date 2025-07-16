namespace Social.Infrastructure.Communication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ServiceBusOptions
{
    public string Route { get; set; } = null!;
    public string[]? Subscriptions { get; set; }
}