using Titeenipeli.Models;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Extensions;

public static class HttpContextExtensions
{
    public static JwtClaim? GetUser(this HttpContext context, JwtService jwtService)
    {
        return context.Items[jwtService.GetJwtClaimName()] as JwtClaim;
    }

    public static User? GetUser(this HttpContext context, JwtService jwtService, IUserRepositoryService userRepository)
    {
        var jwtClaim = context.GetUser(jwtService);
        if (jwtClaim is null) return null;
        return userRepository.GetById(jwtClaim.Id);
    }
}