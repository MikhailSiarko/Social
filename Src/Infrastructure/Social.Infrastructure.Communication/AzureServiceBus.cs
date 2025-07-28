using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Infrastructure.Communication.Abstractions;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Infrastructure.Communication;

public sealed class AzureServiceBus(
    ServiceBusOptions options,
    IConfiguration configuration,
    IHostEnvironment environment,
    IServiceScopeFactory serviceScopeFactory) : IServiceBus, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, List<Type>> _handlers = new();
    private readonly HashSet<Type> _eventTypes = [];

    private readonly ServiceBusClient _serviceBusClient = new(configuration.GetConnectionString("Messaging")!,
        new ServiceBusClientOptions
        {
            Identifier = environment.ApplicationName
        });

    private readonly List<ServiceBusProcessor> _processors = [];

    public async Task<Result<Unit>> PublishAsync(Message message, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            await using var sender = _serviceBusClient.CreateSender(options.Service);
            var messageType = message.GetType();
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message, messageType));
            serviceBusMessage.ApplicationProperties.Add("MessageType", messageType.Name);
            await sender.SendMessageAsync(serviceBusMessage, cts.Token);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error publishing message: {MessageType}", message);
        }
    }

    public async Task<Result<Unit>> SubscribeAsync<TMessage, TMessageHandler>(
        CancellationToken cancellationToken = default)
        where TMessage : Message where TMessageHandler : IMessageHandler<TMessage>
    {
        if (options.Subscriptions == null || options.Subscriptions.Length == 0)
        {
            return Unit.Value;
        }

        RegisterMessageHandler<TMessage, TMessageHandler>();
        try
        {
            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
            var processors = new List<(ServiceBusProcessor Processor, CancellationTokenSource Cts)>();
            foreach (var subscription in options.Subscriptions)
            {
                var processor = _serviceBusClient.CreateProcessor(options.Service, subscription);
                processor.ProcessMessageAsync += ProcessMessageAsync;
                processor.ProcessErrorAsync += ProcessErrorAsync;
                processors.Add((processor, cts));
            }

            await Task.WhenAll(processors.Select(x => x.Processor.StartProcessingAsync(x.Cts.Token)));
            _processors.AddRange(processors.Select(x => x.Processor));
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error starting message processors: {MessageType}", typeof(TMessageHandler).Name);
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
        var message = Encoding.UTF8.GetString(e.Message.Body.ToArray());
        var messageType = e.Message.ApplicationProperties["MessageType"].ToString();

        if (messageType == null || !_handlers.ContainsKey(messageType))
        {
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(e.CancellationToken,
            _cancellationTokenSource.Token);
        var result = await ProcessEvent(messageType, message, cts.Token);

        if (result.IsOk)
            await e.CompleteMessageAsync(e.Message, cts.Token);
        else
            await e.AbandonMessageAsync(e.Message, cancellationToken: cts.Token);
    }

    private static Task ProcessErrorAsync(ProcessErrorEventArgs e)
    {
        return Task.CompletedTask;
    }

    private async Task<Result<Unit>> ProcessEvent(string messageTypeName, string messageStr,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(messageTypeName, out var subscriptions))
            return Unit.Value;

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tasks = new List<Task>();
            foreach (var handler in
                     subscriptions
                         .Select(subscription => scope.ServiceProvider.GetService(subscription))
                         .Where(x => x != null))
            {
                var messageType = _eventTypes.SingleOrDefault(t => t.Name == messageTypeName);
                var message = JsonSerializer.Deserialize(messageStr, messageType!);
                var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType!);
                tasks.Add((Task)concreteType.GetMethod(nameof(IMessageHandler<>.HandleAsync))
                    ?.Invoke(handler, [message, cancellationToken])!);
            }

            await Task.WhenAll(tasks);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error processing message: {MessageType}", messageStr);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.WhenAll(_processors.Select(async processor =>
        {
            processor.ProcessMessageAsync -= ProcessMessageAsync;
            processor.ProcessErrorAsync -= ProcessErrorAsync;
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }).ToArray());

        _processors.Clear();
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        await _serviceBusClient.DisposeAsync();
    }
}