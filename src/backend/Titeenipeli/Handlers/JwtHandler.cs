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

    public JwtToken GetJwtToken(JwtClaimModel jwtClaim)
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        SigningCredentials signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken tokeOptions = new JwtSecurityToken(_configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"], new List<Claim>
            {
                new Claim("data", JsonSerializer.Serialize(jwtClaim))
            },
            expires: DateTime.Now.AddHours(6), signingCredentials: signinCredentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

        return new JwtToken
        {
            Token = tokenString
        };
    }
}