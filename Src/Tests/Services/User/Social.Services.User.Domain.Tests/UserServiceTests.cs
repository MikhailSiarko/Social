using Moq;
using Social.Services.User.Domain.Dtos;
using Social.Services.User.Domain.Models;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Services;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Tests;

public sealed class UserServiceTests
{
    [Test]
    public async Task CreateUserAsync_UserWithEmailDoesNotExist_AddAsyncCalledOnce()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));
        var userService = new UserService(mock.Object);
        var createUserModel = new CreateUserDto(email);

        // Act
        _ = await userService.CreateUserAsync(createUserModel, CancellationToken.None);

        // Assert
        mock.Verify(x => x.AddAsync(It.IsAny<Domain.Models.User?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateUserAsync_UserWithEmailExists_AddAsyncNeverCalled()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.User { Email = email });
        var userService = new UserService(mock.Object);
        var createUserModel = new CreateUserDto(email);

        // Act
        _ = await userService.CreateUserAsync(createUserModel, CancellationToken.None);

        // Assert
        mock.Verify(x => x.AddAsync(It.IsAny<Domain.Models.User?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task CreateUserAsync_EmailIsInvalid_ReturnsValidationError(string? email)
    {
        // Arrange
        var emailToTest = email!;
        var mock = new Mock<IUserRepository>();
        var userService = new UserService(mock.Object);
        var createUserModel = new CreateUserDto(emailToTest);

        // Act and Assert
        var result = await userService.CreateUserAsync(createUserModel, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.False);
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error, Is.TypeOf<ValidationError>());
        });
    }

    [Test]
    public async Task CreateUserAsync_UserWithEmailDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));
        var userService = new UserService(mock.Object);
        var getUserByEmailModel = new GetUserByEmailDto(email);

        // Act
        var result = await userService.GetUserAsync(getUserByEmailModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.False);
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public async Task GetUserAsync_UserWithEmailExists_ReturnsUser()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.User { Email = email });
        var userService = new UserService(mock.Object);
        var getUserByEmailModel = new GetUserByEmailDto(email);

        // Act
        var result = await userService.GetUserAsync(getUserByEmailModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.True);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.Not.Null);
        });
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task GetUserAsync_EmailIsInvalid_ReturnsValidationError(string? email)
    {
        // Arrange
        var emailToTest = email!;
        var mock = new Mock<IUserRepository>();
        var userService = new UserService(mock.Object);
        var getUserByEmailModel = new GetUserByEmailDto(emailToTest);

        // Act
        var result = await userService.GetUserAsync(getUserByEmailModel, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.False);
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error, Is.TypeOf<ValidationError>());
        });
    }
}