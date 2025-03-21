using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;
using Titeenipeli.Options;

namespace Titeenipeli.Services;

public sealed class JwtService : IJwtService
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
            Guild = user.Guild.Name
        };
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string GetJwtToken(JwtClaim jwtClaim)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var encryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Encryption));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var encryptingCredentials = new EncryptingCredentials(encryptionKey,
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes256CbcHmacSha512);

        List<Claim> claims =
        [
            new(_jwtOptions.ClaimName, JsonSerializer.Serialize(jwtClaim))
        ];


        var tokeOptions = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
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
            HttpOnly = false,
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