using DomainUser = Social.Services.User.Domain.Models.User;
using PersistenceUser = Social.Services.User.Persistence.Models.User;

namespace Social.Services.User.Persistence;

public static class Converter
{
    public static DomainUser Convert(PersistenceUser user)
    {
        return new DomainUser
        {
            Id = user.Id,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            FollowersCount = user.Followers,
            FollowingsCount = user.Followings,
        };
    }

    public static PersistenceUser Convert(DomainUser user)
    {
        return new PersistenceUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            AvatarUrl = user.AvatarUrl
        };
    }
}