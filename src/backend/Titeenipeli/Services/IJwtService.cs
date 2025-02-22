using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IJwtService
{
    JwtClaim CreateJwtClaim(User user);
    string GetJwtToken(JwtClaim jwtClaim);
    string GetJwtToken(User user);

    CookieOptions GetAuthorizationCookieOptions();
    string GetAuthorizationCookieName();
    string GetJwtClaimName();
}