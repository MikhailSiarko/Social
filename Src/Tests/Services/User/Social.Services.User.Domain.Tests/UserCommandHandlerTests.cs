using Moq;
using Social.Services.User.Domain.Commands;
using Social.Services.User.Domain.Persistence;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Tests;

public sealed class UserCommandHandlerTests
{
    [Test]
    public async Task UserCommandHandler_UserWithEmailDoesNotExist_AddAsyncCalledOnce()
    {
        // Arrange
        const string email = "test+1@mail.com";
        const string password = "test123456";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));
        var commandHandler = new UserCommandHandler(mock.Object);
        var createUserCommand = new CreateUserCommand(email, password);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        mock.Verify(x => x.AddAsync(It.IsAny<Domain.Models.User?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UserCommandHandler_UserWithEmailExists_AddAsyncNeverCalled()
    {
        // Arrange
        const string email = "test+1@mail.com";
        const string password = "test123456";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.User { Email = email, Password = "qwerty" });
        var commandHandler = new UserCommandHandler(mock.Object);
        var createUserCommand = new CreateUserCommand(email, password);

        // Act
        _ = await commandHandler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        mock.Verify(x => x.AddAsync(It.IsAny<Domain.Models.User?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task UserCommandHandler_EmailIsInvalid_ReturnsValidationError(string? email)
    {
        // Arrange
        var emailToTest = email!;
        const string password = "test123456";
        var mock = new Mock<IUserRepository>();
        var commandHandler = new UserCommandHandler(mock.Object);
        var createUserCommand = new CreateUserCommand(emailToTest, password);

        // Act and Assert
        var result = await commandHandler.Handle(createUserCommand, CancellationToken.None);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.False);
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error, Is.TypeOf<ValidationError>());
        });
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task UserCommandHandler_PasswordIsInvalid_ThrowsValidationException(string? password)
    {
        // Arrange
        const string email = "test+1@mail.com";
        var passwordToTest = password!;
        var mock = new Mock<IUserRepository>();
        var commandHandler = new UserCommandHandler(mock.Object);
        var createUserCommand = new CreateUserCommand(email, passwordToTest);

        // Act and Assert
        var result = await commandHandler.Handle(createUserCommand, CancellationToken.None);
        
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