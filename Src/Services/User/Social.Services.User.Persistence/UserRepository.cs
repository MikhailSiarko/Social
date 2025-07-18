using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Social.Services.User.Domain.Persistence;
using Social.Shared;
using Social.Shared.Errors;
using DomainUser = Social.Services.User.Domain.Models.User;
using PersistenceUser = Social.Services.User.Persistence.Models.User;

namespace Social.Services.User.Persistence;

public sealed class UserRepository(IConfiguration configuration) : RepositoryBase(configuration), IUserRepository
{
    private readonly IConfiguration _configuration = configuration;

    protected override string ContainerName => _configuration["Containers:Users:Name"]!;
    protected override string DatabaseName => _configuration["Database"]!;
    private string PartitionKeyPath => _configuration["Containers:Users:PartitionKeyPath"]!;

    protected override async Task<Container?> SetupAsync(Database database,
        CancellationToken cancellationToken = default)
    {
        return await database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            Id = ContainerName,
            PartitionKeyPath = PartitionKeyPath,
            UniqueKeyPolicy = new UniqueKeyPolicy { UniqueKeys = { new UniqueKey { Paths = { "/email" } } } }
        }, cancellationToken: cancellationToken);
    }

    public async Task<Result<DomainUser>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var userIdString = id.ToString();
            var userResponse = await container.ReadItemAsync<PersistenceUser>(userIdString,
                new PartitionKey(userIdString), cancellationToken: cancellationToken);
            if (userResponse is null)
                return new Error("Read result is null. User ID = '{UserId}'", userIdString);

            var user = userResponse.Resource;
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
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.email = @email")
                .WithParameter("@email", email);

            using var iterator = container.GetItemQueryIterator<PersistenceUser>(queryDefinition);
            var result = new List<PersistenceUser>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                result.AddRange(response);
            }

            var user = result.Select(Converter.Convert).SingleOrDefault();
            if (user is null)
                return new NotFound("User with email '{Email}' not found.", email);

            return user;
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
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var persistenceUser = Converter.Convert(user);
            persistenceUser.CreatedAt = DateTime.UtcNow;
            await container.CreateItemAsync(persistenceUser, new PartitionKey(user.Id.ToString()),
                cancellationToken: cancellationToken);
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
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var userIdString = userId.ToString();
            var userResponse = await container.ReadItemAsync<PersistenceUser>(userIdString,
                new PartitionKey(userIdString), cancellationToken: cancellationToken);
            if (userResponse is null)
                return new Error("Read result is null. User ID = '{UserId}'", userIdString);

            var user = userResponse.Resource;
            if (user is null)
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
            var containerResult = await GetContainerAsync(cancellationToken);
            if (containerResult.IsError)
                return containerResult.Error;

            var container = containerResult.Value;
            var userIdString = userId.ToString();
            var followsToUserIdString = followsToUserId.ToString();
            await Task.WhenAll(
                container.PatchItemAsync<PersistenceUser>(userIdString, new PartitionKey(userIdString),
                    [PatchOperation.Increment("/followings", count)], cancellationToken: cancellationToken),
                container.PatchItemAsync<PersistenceUser>(followsToUserIdString,
                    new PartitionKey(followsToUserIdString), [PatchOperation.Increment("/followers", count)],
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