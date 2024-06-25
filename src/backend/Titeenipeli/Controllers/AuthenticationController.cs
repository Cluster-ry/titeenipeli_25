using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Handlers;
using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApiDbContext _dbContext;

    public AuthenticationController(IConfiguration configuration, ApiDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromBody] Login login)
    {
        // TODO: Actual login validation 
        if (login is not { Username: "test", Password: "test123" })
        {
            return Unauthorized();
        }

        User? user = _dbContext.Users.Include(user => user.Guild).FirstOrDefault(user => user.Code == login.Username);

        if (user == null)
        {
            return BadRequest();
        }

        JwtClaimModel jwtClaim = new JwtClaimModel
        {
            Id = user.Id,
            CoordinateOffset = new CoordinateModel
            {
                X = user.SpawnX,
                Y = user.SpawnY
            },
            GuildId = user.Guild.Color
        };

        Response.Cookies.Append("X-Authorization", new JwtHandler(_configuration).GetJwtToken(jwtClaim),
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromHours(6)
            });

        return Ok();
    }
}