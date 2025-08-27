using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Social.Infrastructure.Communication.Abstractions;
using Social.Shared;
using Social.Shared.Errors;
using Error = Social.Shared.Errors.Error;

namespace Social.Infrastructure.Communication;

public sealed class KafkaServiceBus(
    ServiceBusOptions options,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<KafkaServiceBus> logger,
    IServiceScopeFactory serviceScopeFactory) : IServiceBus, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, List<Type>> _handlers = new();
    private readonly HashSet<Type> _eventTypes = [];
    private bool _isTopicCreated;

    private readonly ProducerConfig _producerConfig = new()
    {
        ClientId = environment.ApplicationName,
        BootstrapServers = configuration.GetConnectionString("Messaging")
    };

    public async Task<Result<Unit>> PublishAsync(Message message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_isTopicCreated)
                await InitializeAsync();

            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            using var producer =
                new ProducerBuilder<string, string>(_producerConfig).Build();
            var messageType = message.GetType();
            await producer.ProduceAsync(options.Topic, new Message<string, string>
            {
                Key = message.CorrelationId.ToString(),
                Value = JsonSerializer.Serialize(message, messageType),
                Headers = [new Header("MessageType", Encoding.UTF8.GetBytes(messageType.Name))]
            }, cts.Token);
            logger.LogTrace("Message published: {MessageType}", messageType.Name);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while publishing message: {MessageType}", message.GetType().Name);
        }
    }

    public async Task<Result<Unit>> SubscribeAsync<TMessage, TMessageHandler>(
        CancellationToken cancellationToken = default)
        where TMessage : Message where TMessageHandler : IMessageHandler<TMessage>
    {
        if (!_isTopicCreated)
            await InitializeAsync();

        RegisterMessageHandler<TMessage, TMessageHandler>();
        try
        {
            using var cts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

            ConsumerConfig consumerConfig = new()
            {
                GroupId = environment.ApplicationName,
                ClientId = $"{environment.ApplicationName}-{typeof(TMessageHandler).Name}",
                BootstrapServers = configuration.GetConnectionString("Messaging"),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(options.Topic);

            while (!cts.IsCancellationRequested)
            {
                var result = consumer.Consume(cts.Token);
                if (result?.Message?.Value is null)
                    continue;

                var processResult = await ProcessMessageAsync(result.Message, cts.Token);
                if (processResult.IsError)
                {
                    logger.LogError(processResult.Error);
                    continue;
                }

                logger.LogTrace("Message processed: {MessageType}", result.Message.Key);
            }

            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while processing message: {MessageType}", typeof(TMessageHandler).Name);
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
            handlers = [typeof(TMessageHandler)];
            _handlers.Add(messageTypeName, handlers);
        }
    }

    private Task<Result<Unit>> ProcessMessageAsync(Message<string, string> message,
        CancellationToken token = default)
    {
        var messageTypeHeader = message.Headers.SingleOrDefault(h => h.Key == "MessageType");
        if (messageTypeHeader is null)
            return Task.FromResult<Result<Unit>>(
                new Error("Message header 'MessageType' not found. Message: {MessageKey}", message.Key));

        var messageType = Encoding.UTF8.GetString(messageTypeHeader.GetValueBytes());
        return _handlers.ContainsKey(messageType)
            ? ProcessEventAsync(messageType, message.Value, token)
            : Task.FromResult<Result<Unit>>(Unit.Value);
    }

    private async Task InitializeAsync()
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig
                { BootstrapServers = configuration.GetConnectionString("Messaging") }).Build();
            await admin.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = options.Topic,
                    NumPartitions = Convert.ToInt32(configuration["Kafka:Partitions"])
                }
            ]);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error while creating topic: {Topic}", options.Topic);
        }
        finally
        {
            _isTopicCreated = true;
        }
    }

    private async Task<Result<Unit>> ProcessEventAsync(string messageTypeName, string messageStr,
        CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(messageTypeName, out var subscriptions))
            return Unit.Value;

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tasks = new List<Task<Result<Unit>>>();
            foreach (var handler in
                     subscriptions
                         .Select(scope.ServiceProvider.GetService)
                         .Where(x => x != null))
            {
                var messageType = _eventTypes.SingleOrDefault(t => t.Name == messageTypeName);
                var message = JsonSerializer.Deserialize(messageStr, messageType!);
                var concreteType = typeof(IMessageHandler<>).MakeGenericType(messageType!);
                tasks.Add((Task<Result<Unit>>)concreteType.GetMethod(nameof(IMessageHandler<>.HandleAsync))
                    ?.Invoke(handler, [message, cancellationToken])!);
            }

            await Task.WhenAll(tasks);
            return tasks.Any(t => t.Result.IsError)
                ? tasks.First(t => t.Result.IsError).Result
                : Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error processing message: {MessageType}", messageStr);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
    }
}