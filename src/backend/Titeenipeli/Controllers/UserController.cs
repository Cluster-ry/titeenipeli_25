using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Results;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IGuildRepositoryService _guildRepositoryService;
    private readonly JwtService _jwtService;
    private readonly IUserRepositoryService _userRepositoryService;

    public UserController(JwtService jwtService,
                          IUserRepositoryService userRepositoryService,
                          IGuildRepositoryService guildRepositoryService)
    {
        _jwtService = jwtService;
        _userRepositoryService = userRepositoryService;
        _guildRepositoryService = guildRepositoryService;
    }

    [HttpPost]
    public IActionResult PostUsers([FromBody] PostUsersInput usersInput)
    {
        User? user = _userRepositoryService.GetByTelegramId(usersInput.Id);

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

            _userRepositoryService.Add(user);
        }

        Response.Cookies.AppendJwtCookie(_jwtService, user);

        return Ok(new PostUserResult
        {
            Guild = user.Guild?.Color.ToString()
        });
    }

    [HttpPut]
    [Authorize]
    public IActionResult PutUsers([FromBody] PutUsersInput input)
    {
        JwtClaim? jwtClaim = HttpContext.GetUser(_jwtService);

        bool validGuild = int.TryParse(input.Guild, out int guildColor);

        if (jwtClaim == null || !validGuild)
        {
            return BadRequest();
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);
        Guild? guild = _guildRepositoryService.GetByColor(guildColor);

        if (user == null || guild == null || user.Guild != null)
        {
            return BadRequest();
        }

        user.Guild = guild;
        _userRepositoryService.Update(user);

        // Update the claim because users guild has changed
        Response.Cookies.AppendJwtCookie(_jwtService, user);
        return Ok();
    }
}