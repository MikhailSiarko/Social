using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Social.Services.User.Domain.Persistence;
using Social.Shared;
using Social.Shared.Errors;
using DomainUser = Social.Services.User.Domain.Models.User;
using PersistenceUser = Social.Services.User.Persistence.Models.User;

namespace Social.Services.User.Persistence;

public sealed class UserRepository(IConfiguration configuration)
    : RepositoryBase<PersistenceUser>(configuration), IUserRepository
{
    private readonly IConfiguration _configuration = configuration;

    protected override string CollectionName => _configuration["Collections:Users"]!;
    protected override string DatabaseName => _configuration["Database"]!;

    protected override Task SetupAsync(IMongoCollection<PersistenceUser> collection,
        CancellationToken cancellationToken = default)
    {
        var uniqueIndex = new CreateIndexModel<PersistenceUser>(
            Builders<PersistenceUser>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "Users_Email_Unique" });
        return collection.Indexes.CreateOneAsync(uniqueIndex, cancellationToken: cancellationToken);
    }

    public async Task<Result<DomainUser>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            var userCursor = await collection.FindAsync(x => x.Id == id,
                new FindOptions<PersistenceUser> { Limit = 1 }, cancellationToken: cancellationToken);
            if (userCursor is null)
                return new Error("Read result is null. User ID = '{UserId}'", id);

            var user = await userCursor.SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                return new NotFound("User with ID '{0}' not found.", id);

            return Converter.Convert(user);
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while getting user with ID '{UserId}'", id);
        }
    }

    public async Task<Result<DomainUser>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(email);
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            var userCursor = await collection.FindAsync(x => x.Email == email,
                new FindOptions<PersistenceUser> { Limit = 1 }, cancellationToken: cancellationToken);
            if (userCursor is null)
                return new Error("Read result is null. User Email = '{Email}'", email);

            var user = await userCursor.SingleOrDefaultAsync(cancellationToken);
            if (user is null)
                return new NotFound("User with Email '{Email}' not found.", email);

            return Converter.Convert(user);
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while getting user with email '{Email}'", email);
        }
    }

    public async Task<Result<Unit>> AddAsync(DomainUser? user, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(user);
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            var persistenceUser = Converter.Convert(user);
            persistenceUser.CreatedAt = DateTime.UtcNow;
            await collection.InsertOneAsync(persistenceUser, cancellationToken: cancellationToken);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while adding user with email '{Email}'", user!.Email);
        }
    }

    public async Task<Result<Unit>> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            var count = await collection.CountDocumentsAsync(x => x.Id == userId, cancellationToken: cancellationToken);
            if (count == 0)
                return new NotFound("User with ID '{0}' not found.", userId);

            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while getting user with ID '{UserId}'", userId);
        }
    }

    public async Task<Result<Unit>> UpdateFollowInfoAsync(Guid userId, Guid followsToUserId, bool unfollow = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = unfollow ? -1 : 1;
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            await Task.WhenAll(
                collection.UpdateOneAsync(x => x.Id == userId,
                    Builders<PersistenceUser>.Update.Inc(x => x.Followings, count),
                    cancellationToken: cancellationToken),
                collection.UpdateOneAsync(x => x.Id == followsToUserId,
                    Builders<PersistenceUser>.Update.Inc(x => x.Followers, count),
                    cancellationToken: cancellationToken));

            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e,
                "Error while updating user's followers count: UserId = '{UserId}', follows to User Id {FollowsToUserID}",
                userId, followsToUserId);
        }
    }
}