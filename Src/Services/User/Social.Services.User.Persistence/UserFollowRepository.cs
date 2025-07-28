using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Persistence.Models;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Persistence;

public sealed class UserFollowRepository(IConfiguration configuration)
    : RepositoryBase(configuration), IUserFollowRepository
{
    private readonly IConfiguration _configuration = configuration;

    protected override string ContainerName => _configuration["Containers:UserFollows:Name"]!;
    protected override string DatabaseName => _configuration["Database"]!;
    private string PartitionKeyPath => _configuration["Containers:UserFollows:PartitionKeyPath"]!;

    protected override async Task<Container?> SetupAsync(Database database,
        CancellationToken cancellationToken = default)
    {
        return await database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            Id = ContainerName,
            PartitionKeyPath = PartitionKeyPath,
            UniqueKeyPolicy = new UniqueKeyPolicy
                { UniqueKeys = { new UniqueKey { Paths = { "/userId", "/followsToUserId" } } } }
        }, cancellationToken: cancellationToken);
    }

    public async Task<Result<Unit>> AddAsync(Guid userId, Guid followedByUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var userFollow = new UserFollow { UserId = userId, FollowsToUserId = followedByUserId, StartedFollowAt = DateTime.UtcNow };
            await container.CreateItemAsync(
                userFollow,
                new PartitionKey(userFollow.Id),
                cancellationToken: cancellationToken);
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
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var userFollow = new UserFollow { UserId = userId, FollowsToUserId = followedByUserId };
            await container.DeleteItemAsync<UserFollow>(userFollow.Id, new PartitionKey(userFollow.Id),
                cancellationToken: cancellationToken);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return new Failure(e, "Error while removing user follow: {UserID} - {FollowedByUserId}", userId,
                followedByUserId);
        }
    }
}