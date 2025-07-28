using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Social.Infrastructure.DependencyInjection;
using Social.Services.User.Persistence;

namespace Social.Services.User.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Auth:Issuer"],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Auth:Key"]!))
            };
        });

        return services.AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IApplicationService, ApplicationService>()
            .AddHttpContextAccessor()
            .AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Domain.Models.User>())
            .AddServiceBus(configuration)
            .AddPersistence();
    }
}
