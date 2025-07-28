using FluentValidation;
using Moq;
using Social.Services.User.Domain.Persistence;
using Social.Services.User.Domain.Queries;
using Social.Shared.Errors;

namespace Social.Services.User.Domain.Tests;

public sealed class UserQueryHandlerTests
{
    [Test]
    public async Task UserQueryHandler_UserWithEmailDoesNotExist_ReturnsNull()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound(null!));
        var queryHandler = new UserQueryHandler(mock.Object);
        var getUserByEmail = new GetUserByEmailQuery(email);

        // Act
        var result = await queryHandler.Handle(getUserByEmail, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsOk, Is.False);
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Value, Is.Null);
        });
    }

    [Test]
    public async Task UserQueryHandler_UserWithEmailExists_ReturnsUser()
    {
        // Arrange
        const string email = "test+1@mail.com";
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.User { Email = email, Password = "qwerty" });
        var queryHandler = new UserQueryHandler(mock.Object);
        var getUserByEmailQuery = new GetUserByEmailQuery(email);

        // Act
        var result = await queryHandler.Handle(getUserByEmailQuery, CancellationToken.None);

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
    public async Task UserQueryHandler_EmailIsInvalid_ReturnsValidationError(string? email)
    {
        // Arrange
        var emailToTest = email!;
        var mock = new Mock<IUserRepository>();
        var queryHandler = new UserQueryHandler(mock.Object);
        var getUserByEmailQuery = new GetUserByEmailQuery(emailToTest);

        // Act
        var result = await queryHandler.Handle(getUserByEmailQuery, CancellationToken.None);

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