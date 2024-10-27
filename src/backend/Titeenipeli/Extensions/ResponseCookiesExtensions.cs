using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Services;

namespace Titeenipeli.Extensions;

public static class ResponseCookiesExtensions
{
    public static void AppendJwtCookie(this IResponseCookies cookies, JwtService jwtService, User user)
    {
        cookies.Append(jwtService.GetAuthorizationCookieName(), jwtService.GetJwtToken(user),
            jwtService.GetAuthorizationCookieOptions());
    }
}