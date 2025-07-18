using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Social.Services.User.Application;

public sealed class AuthenticationService(IConfiguration configuration) : IAuthenticationService
{
    public string Authenticate(Domain.Models.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return EncodeSecurityToken(user);
    }

    private string EncodeSecurityToken(Domain.Models.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var claimsIdentity = GetIdentity(user);
        var jwt = GenerateToken(claimsIdentity.Claims);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(jwt);
    }

    private static ClaimsIdentity GetIdentity(Domain.Models.User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var claimsIdentity =
            new ClaimsIdentity(claims, "Token");
        return claimsIdentity;
    }

    private JwtSecurityToken GenerateToken(IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        return new JwtSecurityToken(
            configuration["Auth:Issuer"],
            notBefore: now,
            claims: claims,
            expires: now.Add(TimeSpan.FromMinutes(Convert.ToInt64(configuration["Auth:Lifetime"]))),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Auth:Key"]!)),
                SecurityAlgorithms.HmacSha256));
    }
}
