namespace Social.Infrastructure.Communication;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ServiceBusOptions
{
    public string Key { get; set; } = null!;
    public string Topic { get; set; } = null!;
}
