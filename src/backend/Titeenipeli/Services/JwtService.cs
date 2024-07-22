using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Services;

public class JwtService
{
    private readonly JwtOptions _jwtOptions;

    public JwtService(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public JwtClaim CreateJwtClaim(User user)
    {
        return new JwtClaim
        {
            Id = user.Id,
            CoordinateOffset = new Coordinate
            {
                X = user.SpawnX,
                Y = user.SpawnY
            },
            GuildId = user.Guild?.Color
        };
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string GetJwtToken(JwtClaim jwtClaim)
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        SymmetricSecurityKey encryptionKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Encryption));

        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        EncryptingCredentials encryptingCredentials = new EncryptingCredentials(encryptionKey,
            SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

        List<Claim> claims = [new Claim(_jwtOptions.ClaimName, JsonSerializer.Serialize(jwtClaim))];

        JwtSecurityToken tokeOptions = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
            _jwtOptions.ValidIssuer,
            _jwtOptions.ValidAudience,
            new ClaimsIdentity(claims),
            DateTime.Now,
            DateTime.Now.AddDays(_jwtOptions.ExpirationDays),
            DateTime.Now,
            signingCredentials,
            encryptingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    }

    public string GetJwtToken(User user)
    {
        return GetJwtToken(CreateJwtClaim(user));
    }

    public CookieOptions GetAuthorizationCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(_jwtOptions.ExpirationDays)
        };
    }

    public string GetAuthorizationCookieName()
    {
        return _jwtOptions.CookieName;
    }

    public string GetJwtClaimName()
    {
        return _jwtOptions.ClaimName;
    }
}