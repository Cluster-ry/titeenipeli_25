using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;

namespace Titeenipeli.Handlers;

public class JwtHandler
{
    private readonly IConfiguration _configuration;

    public JwtHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetJwtToken(JwtClaimModel jwtClaim)
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        SymmetricSecurityKey encryptionKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Encryption"]!));

        SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        EncryptingCredentials encryptingCredentials = new EncryptingCredentials(encryptionKey,
            SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

        List<Claim> claims = [new Claim("data", JsonSerializer.Serialize(jwtClaim))];

        JwtSecurityToken tokeOptions = new JwtSecurityTokenHandler().CreateJwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            new ClaimsIdentity(claims),
            DateTime.Now,
            DateTime.Now.AddDays(int.Parse(_configuration["JWT:ExpirationDays"]!)),
            DateTime.Now,
            signingCredentials,
            encryptingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    }
}