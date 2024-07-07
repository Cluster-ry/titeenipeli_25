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
            entity.TelegramId == input.Id);

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

        JwtHandler jwtHandler = new JwtHandler(_jwtOptions);

        Response.Cookies.Append(_jwtOptions.CookieName, jwtHandler.GetJwtToken(user),
            jwtHandler.GetAuthorizationCookieOptions());

        return Ok(new PostUserResult
        {
            Guild = user.Guild?.Color.ToString()
        });
    }
}