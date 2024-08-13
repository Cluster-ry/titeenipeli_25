using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Enums;
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
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly JwtService _jwtService;
    private readonly SpawnGeneratorService _spawnGeneratorService;

    public UserController(JwtService jwtService,
                          SpawnGeneratorService spawnGeneratorService,
                          IUserRepositoryService userRepositoryService,
                          IGuildRepositoryService guildRepositoryService)
    {
        _jwtService = jwtService;
        _spawnGeneratorService = spawnGeneratorService;
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

                SpawnX = -1,
                SpawnY = -1,

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
            Guild = user.Guild?.NameId.ToString()
        });
    }

    [HttpPut]
    [Authorize]
    public IActionResult PutUsers([FromBody] PutUsersInput input)
    {
        JwtClaim? jwtClaim = HttpContext.GetUser(_jwtService);

        bool validGuild = Enum.TryParse(input.Guild, out GuildName guildName);

        if (jwtClaim == null)
        {
            return BadRequest();
        }

        if (!validGuild)
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid guild",
                Code = 400,
                Description = "Provide valid guild"
            };

            return BadRequest(error);
        }

        User? user = _userRepositoryService.GetById(jwtClaim.Id);
        Guild? guild = _guildRepositoryService.GetByNameId(guildName);

        if (user == null || guild == null || user.Guild != null)
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid guild",
                Code = 400,
                Description = "Provide valid guild"
            };

            return BadRequest(error);
        }

        Coordinate spawnPoint = _spawnGeneratorService.GetSpawnPoint(guildName);

        user.SpawnX = spawnPoint.X;
        user.SpawnY = spawnPoint.Y;
        user.Guild = guild;

        _userRepositoryService.Update(user);

        // Update the claim because users guild has changed
        Response.Cookies.AppendJwtCookie(_jwtService, user);
        return Ok();
    }
}