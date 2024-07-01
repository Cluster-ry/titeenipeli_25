using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;
using Titeenipeli.Options;

namespace Titeenipeli.Handlers;

public class JwtHandler
{
    private readonly JwtOptions _jwtOptions;

    public JwtHandler(JwtOptions jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }

    public string GetJwtToken(JwtClaimModel jwtClaim)
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        SymmetricSecurityKey encryptionKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Encryption));

        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        EncryptingCredentials encryptingCredentials = new EncryptingCredentials(encryptionKey,
            SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

        List<Claim> claims = [new Claim("data", JsonSerializer.Serialize(jwtClaim))];

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
}