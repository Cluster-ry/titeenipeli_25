using System.Security.Claims;
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
    public IActionResult PostUsers([FromBody] PostUsersInput usersInput)
    {
        User? user = _dbContext.Users.Include(entity => entity.Guild).FirstOrDefault(entity =>
            entity.TelegramId == usersInput.Id);

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
                TelegramId = usersInput.Id,
                FirstName = usersInput.FirstName,
                LastName = usersInput.LastName,
                Username = usersInput.Username,
                PhotoUrl = usersInput.PhotoUrl,
                AuthDate = usersInput.AuthDate,
                Hash = usersInput.Hash
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        JwtHandler jwtHandler = new JwtHandler(_jwtOptions);

        Response.Cookies.Append(_jwtOptions.CookieName, jwtHandler.GetJwtToken(user),
            jwtHandler.GetAuthorizationCookieOptions());

        return Ok(new PostUserResult
        {
            Guild = user.Guild?.Color.ToString()
        });
    }

    [HttpPut]
    [Authorize]
    public IActionResult PutUsers([FromBody] PutUsersInput input)
    {
        ClaimsIdentity identity = (ClaimsIdentity)HttpContext.User.Identity!;
        JwtClaim? jwtClaim = JwtHandler.GetJwtClaimFromIdentity(identity);

        User? user = _dbContext.Users.Include(user => user.Guild)
                               .FirstOrDefault(user => jwtClaim != null && user.Id == jwtClaim.Id);

        Guild? guild = _dbContext.Guilds.FirstOrDefault(guild => guild.Color.ToString() == input.Guild);

        if (user == null || guild == null || user.Guild != null)
        {
            return BadRequest();
        }

        user.Guild = guild;
        _dbContext.SaveChanges();


        // Update the claim because users guild has changed
        JwtHandler jwtHandler = new JwtHandler(_jwtOptions);

        Response.Cookies.Append(_jwtOptions.CookieName, jwtHandler.GetJwtToken(user),
            jwtHandler.GetAuthorizationCookieOptions());

        return Ok();
    }
}