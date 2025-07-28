using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Persistence;

public abstract class RepositoryBase(IConfiguration configuration) : IDisposable
{
    private readonly CosmosClient _cosmosClient = new(configuration.GetConnectionString("UserStorage")!,
        new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            CosmosClientTelemetryOptions = new CosmosClientTelemetryOptions
            {
                QueryTextMode = QueryTextMode.All
            }
        });

    protected abstract string ContainerName { get; }
    protected abstract string DatabaseName { get; }

    private Container? _container;

    protected async Task<Result<Container>> GetContainerAsync(CancellationToken cancellationToken = default)
    {
        if (_container != null)
            return _container;

        var database =
            await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName, cancellationToken: cancellationToken);
        if (database == null)
            return new Error("Error while creating database: {DatabaseName}", DatabaseName);

        _container = await SetupAsync(database, cancellationToken);
        if (_container == null)
            return new Error("Error while creating container: {ContainerName}", ContainerName);

        return _container;
    }

    protected abstract Task<Container?> SetupAsync(Database database, CancellationToken cancellationToken = default);

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cosmosClient.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RepositoryBase()
    {
        Dispose(false);
    }
}