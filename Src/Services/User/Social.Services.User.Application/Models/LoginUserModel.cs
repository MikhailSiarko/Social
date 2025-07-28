using System.ComponentModel.DataAnnotations;

namespace Social.Services.User.Application.Models;

public sealed class LoginUserModel([Required][EmailAddress]string email, [Required]string password)
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}