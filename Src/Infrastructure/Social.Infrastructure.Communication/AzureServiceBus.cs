using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;

namespace Social.Infrastructure.Communication;

public sealed class AzureServiceBus(
    ServiceBusOptions options,
    ILogger<AzureServiceBus> logger,
    IConfiguration configuration,
    IHostEnvironment environment,
    IServiceScopeFactory serviceScopeFactory) : IServiceBus, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, List<Type>> _handlers = new();
    private readonly HashSet<Type> _eventTypes = [];

    private readonly ServiceBusClient _serviceBusClient = new(configuration.GetConnectionString("ServiceBus")!,
        new ServiceBusClientOptions
        {
            Identifier = environment.ApplicationName
        });

    private Dictionary<string, ServiceBusProcessor>? _processors;
    private readonly List<CancellationTokenSource> _cancellationTokenSources = [];

    public async Task PublishAsync(Message message, CancellationToken cancellationToken = default)
    {
        if (_cancellationTokenSource.IsCancellationRequested || cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Can't publish a message since a cancellation token is already cancelled: {MessageType}",
                message.GetType().Name);
            return;
        }

        try
        {
            await using var sender = _serviceBusClient.CreateSender(options.Route);
            var messageType = message.GetType();
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message, messageType));
            serviceBusMessage.ApplicationProperties.Add("MessageType", messageType.Name);
            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error publishing message: {Message}", message);
        }
    }

    public async Task SubscribeAsync<TMessage, TMessageHandler>(CancellationToken cancellationToken = default)
        where TMessage : Message where TMessageHandler : IMessageHandler<TMessage>
    {
        if (options.Subscriptions == null || options.Subscriptions.Length == 0)
        {
            return;
        }

        RegisterMessageHandler<TMessage, TMessageHandler>();
        using var cts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
        var processors = new List<(ServiceBusProcessor Processor, CancellationTokenSource Cts)>();
        foreach (var subscription in options.Subscriptions)
        {
            var processor = _serviceBusClient.CreateProcessor(options.Route, subscription);
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;
            processors.Add((processor, cts));
        }

        try
        {
            await Task.WhenAll(processors.Select(x => x.Processor.StartProcessingAsync(x.Cts.Token)));
            _processors = processors.ToDictionary(x => x.Processor.Identifier, x => x.Processor);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error starting message processors: {MessageHandler}", typeof(TMessageHandler).Name);
        }
    }

    private void RegisterMessageHandler<TMessage, TMessageHandler>()
    {
        var messageType = typeof(TMessage);
        _eventTypes.Add(messageType);
        var messageTypeName = messageType.Name;
        if (_handlers.TryGetValue(messageTypeName, out var handlers))
        {
            handlers.Add(typeof(TMessageHandler));
        }
        else
        {
            handlers = [];
            _handlers.Add(messageTypeName, handlers);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs e)
    {
        if (_cancellationTokenSource.IsCancellationRequested || e.CancellationToken.IsCancellationRequested)
            return;

        var message = Encoding.UTF8.GetString(e.Message.Body.ToArray());
        var messageType = e.Message.ApplicationProperties["MessageType"].ToString();

        try
        {
            if (messageType == null || !_handlers.ContainsKey(messageType))
            {
                return;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(e.CancellationToken,
                _cancellationTokenSource.Token);
            _cancellationTokenSources.Add(cts);
            await ProcessEvent(messageType, message, cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message handler error occured while processing message: {MessageType}", messageType);
        }
        finally
        {
            await e.CompleteMessageAsync(e.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs e)
    {
        logger.LogError(e.Exception, "Error processing message");
        return Task.CompletedTask;
    }

    private async Task ProcessEvent(string messageTypeName, string messageStr,
        CancellationToken cancellationToken = default)
    {
        if (_handlers.TryGetValue(messageTypeName, out var subscriptions))
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tasks = new List<Task>();
            foreach (var handler in
                     subscriptions.Select(subscription => scope.ServiceProvider.GetService(subscription)))
            {
                if (handler is null)
                {
                    logger.LogWarning("Handler not found for message type: {MessageType}", messageTypeName);
                    continue;
                }

                var messageType = _eventTypes.SingleOrDefault(t => t.Name == messageTypeName);
                var message = JsonSerializer.Deserialize(messageStr, messageType!);
                var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType!);
                tasks.Add((Task)concreteType.GetMethod(nameof(IMessageHandler<>.HandleAsync))
                    ?.Invoke(handler, [message, cancellationToken])!);
            }

            await Task.WhenAll(tasks);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        foreach (var cancellationTokenSource in _cancellationTokenSources)
        {
            cancellationTokenSource.Dispose();
        }

        if (_processors != null)
        {
            foreach (var processor in _processors.Values)
            {
                processor.ProcessMessageAsync -= ProcessMessageAsync;
                processor.ProcessErrorAsync -= ProcessErrorAsync;
                await processor.StopProcessingAsync();
                await processor.DisposeAsync();
            }

            _processors.Clear();
        }

        await _serviceBusClient.DisposeAsync();
    }
}