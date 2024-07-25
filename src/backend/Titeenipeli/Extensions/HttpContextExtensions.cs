using Titeenipeli.Models;
using Titeenipeli.Services;

namespace Titeenipeli.Extensions;

public static class HttpContextExtensions
{
    public static JwtClaim? GetUser(this HttpContext context, JwtService jwtService)
    {
        return context.Items[jwtService.GetJwtClaimName()] as JwtClaim;
    }
}