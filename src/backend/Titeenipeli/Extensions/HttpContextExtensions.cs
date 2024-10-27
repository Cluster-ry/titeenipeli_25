using Titeenipeli.Models;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Extensions;

public static class HttpContextExtensions
{

    public static User GetUser(this HttpContext context, JwtService jwtService, IUserRepositoryService userRepositoryService)
    {
        var jwtClaim = context.Items[jwtService.GetJwtClaimName()] as JwtClaim;
        if (jwtClaim == null)
        {
            throw new Exception("Missing or invalid authentication cookie.");
        }

        User? user = userRepositoryService.GetById(jwtClaim.Id);
        if (user == null)
        {
            throw new Exception("Couldn't extract user information.");
        }

        return user;
    }
}