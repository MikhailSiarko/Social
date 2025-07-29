using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Persistence.Models;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Persistence;

public sealed class UserFollowRepository(IConfiguration configuration)
    : RepositoryBase<UserFollow>(configuration), IUserFollowRepository
{
    private readonly IConfiguration _configuration = configuration;

    protected override string CollectionName => _configuration["Collections:UserFollows"]!;
    protected override string DatabaseName => _configuration["Database"]!;

    protected override Task SetupAsync(IMongoCollection<UserFollow> collection,
        CancellationToken cancellationToken = default)
    {
        var uniqueIndex = new CreateIndexModel<UserFollow>(
            Builders<UserFollow>.IndexKeys.Combine(
                Builders<UserFollow>.IndexKeys.Ascending(u => u.UserId),
                Builders<UserFollow>.IndexKeys.Ascending(u => u.FollowsToUserId)),
            new CreateIndexOptions { Unique = true, Name = "UserFollows_Unique" });
        return collection.Indexes.CreateOneAsync(uniqueIndex, cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> AddAsync(Guid userId, Guid followedByUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            var userFollow = new UserFollow
                { UserId = userId, FollowsToUserId = followedByUserId, StartedFollowAt = DateTime.UtcNow };
            await collection.InsertOneAsync(userFollow, cancellationToken: cancellationToken);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while adding user follow: {UserID} - {FollowedByUserId}", userId,
                followedByUserId);
        }
    }

    public async Task<Result<Unit>> DeleteAsync(Guid userId, Guid followedByUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var collectionResult = await GetCollectionAsync(cancellationToken);
            if (collectionResult.IsError)
                return collectionResult.Error;

            var collection = collectionResult.Value;
            await collection.DeleteOneAsync(
                x => x.UserId == userId && x.FollowsToUserId == followedByUserId, cancellationToken: cancellationToken);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while removing user follow: {UserID} - {FollowedByUserId}", userId,
                followedByUserId);
        }
    }
}