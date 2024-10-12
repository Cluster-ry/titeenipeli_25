using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Titeenipeli.Enums;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Results;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly BotOptions _botOptions;
    private readonly IGuildRepositoryService _guildRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly SpawnGeneratorService _spawnGeneratorService;

    public UserController(BotOptions botOptions,
                          SpawnGeneratorService spawnGeneratorService,
                          IUserRepositoryService userRepositoryService,
                          IGuildRepositoryService guildRepositoryService)
    {
        _botOptions = botOptions;
        _spawnGeneratorService = spawnGeneratorService;
        _userRepositoryService = userRepositoryService;
        _guildRepositoryService = guildRepositoryService;
    }

    [HttpPost]
    public IActionResult PostUsers([FromBody] PostUsersInput usersInput)
    {
        BadRequestObjectResult? botTokenError = IsBotTokenValid(Request.Headers);
        if (botTokenError != null)
        {
            return botTokenError;
        }

        User? user = _userRepositoryService.GetByTelegramId(usersInput.TelegramId);

        if (user == null)
        {
            BadRequestObjectResult? createUserError = CreateNewUser(usersInput);
            if (createUserError != null)
            {
                return createUserError;
            }
        }

        // Todo: Should return single use authentication token.
        PostUsersResult postUsersResult = new()
        {
            Token = "Test123"
        };
        return Ok(postUsersResult);
    }

    private BadRequestObjectResult? IsBotTokenValid(IHeaderDictionary headers)
    {
        StringValues botToken;
        bool botTokenRetrieved = headers.TryGetValue("X-BOT-KEY", out botToken);
        if (botTokenRetrieved && botToken == _botOptions.Token)
        {
            return null;
        }
        else
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid bot token",
                Code = 403,
                Description = "Bot token is invalid, is client bot at all?"
            };

            return BadRequest(error);
        }
    }

    private BadRequestObjectResult? CreateNewUser(PostUsersInput usersInput)
    {
        bool validGuild = Enum.TryParse(usersInput.Guild, out GuildName guildName);
        Guild? guild = _guildRepositoryService.GetByName(guildName);
        if (!validGuild || guild == null)
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid guild",
                Code = 1414,
                Description = "Provide valid guild"
            };

            return BadRequest(error);
        }

        Coordinate spawnPoint = _spawnGeneratorService.GetSpawnPoint(guildName);

        User user = new User
        {
            Guild = guild,
            Code = "",

            SpawnX = spawnPoint.X,
            SpawnY = spawnPoint.Y,

            TelegramId = usersInput.TelegramId,
            FirstName = usersInput.FirstName,
            LastName = usersInput.LastName,
            Username = usersInput.Username,
        };

        _userRepositoryService.Add(user);

        return null;
    }
}