using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Titeenipeli.Models;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration Configuration;

    public AuthenticationController(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] Login user)
    {
        if (user is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        SymmetricSecurityKey secretKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"] ?? string.Empty));

        SigningCredentials signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken tokeOptions = new JwtSecurityToken(Configuration["JWT:ValidIssuer"],
            Configuration["JWT:ValidAudience"], new List<Claim>(),
            expires: DateTime.Now.AddHours(6), signingCredentials: signinCredentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return Ok(new JwtToken
        {
            Token = tokenString
        });
    }
}