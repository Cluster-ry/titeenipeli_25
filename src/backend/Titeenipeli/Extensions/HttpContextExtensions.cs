using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Services;

namespace Titeenipeli.Extensions;

public static class HttpContextExtensions
{
    public static User GetUser(this HttpContext context, IJwtService jwtService, IUserProvider userProvider)
    {
        if (context.Items[jwtService.GetJwtClaimName()] is not JwtClaim jwtClaim)
        {
            throw new Exception("Missing or invalid authentication cookie.");
        }

        var user = userProvider.GetById(jwtClaim.Id);
        if (user == null)
        {
            throw new Exception("Couldn't extract user information.");
        }

        return user;
    }

    public static User GetCachedUser(this HttpContext context, IJwtService jwtService)
    {
        if (context.Items[jwtService.GetJwtClaimName()] is not JwtClaim jwtClaim)
        {
            throw new Exception("Missing or invalid authentication cookie.");
        }

        var user = new User
        {
            Id = jwtClaim.Id,
            SpawnX = jwtClaim.CoordinateOffset.X,
            SpawnY = jwtClaim.CoordinateOffset.Y,
            Guild = new Guild
            {
                Name = jwtClaim.Guild
            },
            Code = "",
            TelegramId = "",
            FirstName = "",
            LastName = "",
            Username = ""
        };

        return user;
    }
}