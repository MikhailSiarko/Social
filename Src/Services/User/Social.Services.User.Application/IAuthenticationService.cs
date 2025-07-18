namespace Social.Services.User.Application;

public interface IAuthenticationService
{
    string Authenticate(Domain.Models.User user);
}