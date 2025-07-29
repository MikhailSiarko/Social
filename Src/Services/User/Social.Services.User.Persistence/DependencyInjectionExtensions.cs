using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Social.Services.User.Domain.Persistence;

namespace Social.Services.User.Persistence;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserFollowRepository, UserFollowRepository>();
        return services;
    }
}