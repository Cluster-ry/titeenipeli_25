using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;

namespace Titeenipeli.Handlers;

public class LoginHandler
{
    private readonly IConfiguration _configuration;

    public LoginHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtToken GetJwtToken()
    {
        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty));

        SigningCredentials signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken tokeOptions = new JwtSecurityToken(_configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"], new List<Claim>(),
            expires: DateTime.Now.AddHours(6), signingCredentials: signinCredentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

        return new JwtToken
        {
            Token = tokenString
        };
    }
}