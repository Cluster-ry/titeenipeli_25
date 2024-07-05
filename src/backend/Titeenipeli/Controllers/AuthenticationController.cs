using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Handlers;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly ApiDbContext _dbContext;

    public AuthenticationController(JwtOptions jwtOptions, ApiDbContext dbContext)
    {
        _jwtOptions = jwtOptions;
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
            GuildId = user.Guild?.Color
        };

        Response.Cookies.Append(_jwtOptions.CookieName, new JwtHandler(_jwtOptions).GetJwtToken(jwtClaim),
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(_jwtOptions.ExpirationDays)
            });

        return Ok();
    }
}