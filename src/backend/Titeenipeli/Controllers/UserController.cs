using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Handlers;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Results;
using Titeenipeli.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly ApiDbContext _dbContext;

    public UserController(JwtOptions jwtOptions, ApiDbContext dbContext)
    {
        _jwtOptions = jwtOptions;
        _dbContext = dbContext;
    }

    [HttpPost]
    public IActionResult PostUser([FromBody] PostUserInput input)
    {
        User? user = _dbContext.Users.Include(entity => entity.Guild).FirstOrDefault(entity =>
            entity.TelegramId != null && entity.TelegramId == input.Id);

        if (user == null)
        {
            user = new User
            {
                Guild = null,
                Code = "",

                // TODO: Generate users spawn point
                SpawnX = 0,
                SpawnY = 0,

                // TODO: Validate telegram credentials before creating a new user
                TelegramId = input.Id,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Username = input.Username,
                PhotoUrl = input.PhotoUrl,
                AuthDate = input.AuthDate,
                Hash = input.Hash
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
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

        return Ok(new PostUserResult
        {
            Guild = user.Guild?.Color.ToString()
        });
    }

    [HttpPut]
    [Authorize]
    public IActionResult PutUser([FromBody] PutUserInput input)
    {
        ClaimsIdentity identity = (ClaimsIdentity)HttpContext.User.Identity!;
        JwtClaimModel? jwtClaim = JsonSerializer.Deserialize<JwtClaimModel>(identity.FindFirst("data")!.Value);
        User? user = _dbContext.Users.Include(user => user.Guild)
                               .FirstOrDefault(user => jwtClaim != null && user.Id == jwtClaim.Id);

        Guild? guild = _dbContext.Guilds.FirstOrDefault(guild => guild.Color.ToString() == input.Guild);

        if (user == null || guild == null)
        {
            return BadRequest();
        }

        // TODO: Handle the case where user already has a guild
        user.Guild = guild;
        _dbContext.SaveChanges();


        // Update the claim because users guild has changed
        JwtClaimModel newJwtClaim = new JwtClaimModel
        {
            Id = user.Id,
            CoordinateOffset = new CoordinateModel
            {
                X = user.SpawnX,
                Y = user.SpawnY
            },
            GuildId = user.Guild?.Color
        };

        Response.Cookies.Append(_jwtOptions.CookieName, new JwtHandler(_jwtOptions).GetJwtToken(newJwtClaim),
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(_jwtOptions.ExpirationDays)
            });

        return Ok();
    }
}