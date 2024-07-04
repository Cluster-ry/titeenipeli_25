using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Handlers;

public class JwtHandler
{
    private readonly JwtOptions _jwtOptions;

    public JwtHandler(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static JwtClaim CreateJwtClaim(User user)
    {
        return new JwtClaim
        {
            Id = user.Id,
            CoordinateOffset = new CoordinateModel
            {
                X = user.SpawnX,
                Y = user.SpawnY
            },
            GuildId = user.Guild.Color
        };
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string GetJwtToken(JwtClaim jwtClaim, string claimName = "data")
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        SymmetricSecurityKey encryptionKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Encryption));

        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        EncryptingCredentials encryptingCredentials = new EncryptingCredentials(encryptionKey,
            SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

        List<Claim> claims = [new Claim(claimName, JsonSerializer.Serialize(jwtClaim))];

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

    public JwtClaim? GetJwtClaimFromIdentity(ClaimsIdentity identity, string claimName = "data")
    {
        string? json = identity.FindFirst("data")?.Value;
        return json != null ? JsonSerializer.Deserialize<JwtClaim>(json) : null;
    }
}