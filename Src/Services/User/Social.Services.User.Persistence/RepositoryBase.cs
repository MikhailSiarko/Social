using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Persistence;

public abstract class RepositoryBase<TDocument>(IConfiguration configuration) : IDisposable
{
    private readonly MongoClient _mongoClient = new(configuration.GetConnectionString("UserStorage")!);

    protected abstract string CollectionName { get; }
    protected abstract string DatabaseName { get; }

    private IMongoCollection<TDocument>? _collection;

    protected async Task<Result<IMongoCollection<TDocument>>> GetCollectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_collection != null)
            return Result<IMongoCollection<TDocument>>.FromValue(_collection);

        var database = _mongoClient.GetDatabase(DatabaseName);
        if (database == null)
            return new Error("Error while creating database: {DatabaseName}", DatabaseName);

        var collectionsCursor = await database.ListCollectionNamesAsync(
            new ListCollectionNamesOptions
                { Filter = Builders<BsonDocument>.Filter.Eq(x => x.AsString, CollectionName) },
            cancellationToken: cancellationToken);

        if (await collectionsCursor.AnyAsync(cancellationToken: cancellationToken))
        {
            _collection = database.GetCollection<TDocument>(CollectionName);
            return Result<IMongoCollection<TDocument>>.FromValue(_collection);
        }

        _collection = database.GetCollection<TDocument>(CollectionName);
        await SetupAsync(_collection, cancellationToken);
        return Result<IMongoCollection<TDocument>>.FromValue(_collection);
    }

    protected abstract Task SetupAsync(IMongoCollection<TDocument> collection,
        CancellationToken cancellationToken = default);

    private void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        _mongoClient.Dispose();
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