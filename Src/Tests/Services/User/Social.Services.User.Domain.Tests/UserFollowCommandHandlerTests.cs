using Moq;
using Social.Services.User.Domain.Commands;
using Social.Services.User.Domain.Persistence;
using Social.Shared;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Tests;

public class UserFollowCommandHandlerTests
{

    [Test]
    public async Task CreateUserFollow_FollowedUserDoesNotExist_AddAsyncNotCalled()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userRepositoryMock.Setup(x => x.ExistsAsync(followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new CreateUserFollowCommand(userId, followedUserId);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userFollowRepositoryMock.Verify(x => x.AddAsync(userId, followedUserId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CreateUserFollow_FollowedUserDoesNotExist_UpdateFollowInfoAsyncNotCalled()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userRepositoryMock.Setup(x => x.ExistsAsync(followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new CreateUserFollowCommand(userId, followedUserId);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(
            x => x.UpdateFollowInfoAsync(userId, followedUserId, false, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateUserFollow_FollowedUserDoesNotExist_AddAsyncCalledOnce()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userRepositoryMock.Setup(x => x.ExistsAsync(followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new CreateUserFollowCommand(userId, followedUserId);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userFollowRepositoryMock.Verify(x => x.AddAsync(userId, followedUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateUserFollow_FollowedUserDoesNotExist_UpdateFollowInfoAsyncCalledOnce()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userRepositoryMock.Setup(x => x.ExistsAsync(followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new CreateUserFollowCommand(userId, followedUserId);

        // Act
        var result = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(
            x => x.UpdateFollowInfoAsync(userId, followedUserId, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteUserFollow_DeletionFailed_UpdateFollowInfoAsyncNotCalled()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userFollowRepositoryMock.Setup(x => x.DeleteAsync(userId, followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Error("User not found"));

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new DeleteUserFollowCommand(userId, followedUserId);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(x => x.UpdateFollowInfoAsync(userId, followedUserId, true, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteUserFollow_DeletionIsOk_UpdateFollowInfoAsyncCalledOnce()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var followedUserId = Guid.CreateVersion7();
        var userRepositoryMock = new Mock<IUserRepository>();
        var userFollowRepositoryMock = new Mock<IUserFollowRepository>();
        userFollowRepositoryMock.Setup(x => x.DeleteAsync(userId, followedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var commandHandler = new UserFollowCommandHandler(userFollowRepositoryMock.Object, userRepositoryMock.Object);
        var createUserCommand = new DeleteUserFollowCommand(userId, followedUserId);

        // Act
        var result = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(
            x => x.UpdateFollowInfoAsync(userId, followedUserId, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}