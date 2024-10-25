using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Titeenipeli.Enums;
using Titeenipeli.Extensions;
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
public class UserController(
                      IWebHostEnvironment webHostEnvironment,
                      BotOptions botOptions,
                      SpawnGeneratorService spawnGeneratorService,
                      IUserRepositoryService userRepositoryService,
                      IGuildRepositoryService guildRepositoryService,
                      JwtService jwtService) : ControllerBase
{
    private const int _loginTokenLength = 32;

    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly BotOptions _botOptions = botOptions;
    private readonly IGuildRepositoryService _guildRepositoryService = guildRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService = userRepositoryService;
    private readonly SpawnGeneratorService _spawnGeneratorService = spawnGeneratorService;
    private readonly JwtService _jwtService = jwtService;
    private readonly TimeSpan _loginTokenExpiryTime = TimeSpan.FromMinutes(botOptions.LoginTokenExpirationInMinutes);

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
            user = CreateNewUser(usersInput);
            if (user == null)
            {
                return BadRequest(new ErrorResult
                {
                    Title = "Invalid guild",
                    Code = ErrorCode.InvalidGuild,
                    Description = "Provide valid guild"
                });
            }
        }

        string token = CreateNewLoginTokenForUser(user);
        PostUsersResult postUsersResult = new()
        {
            Token = token
        };
        return Ok(postUsersResult);
    }

    [HttpPost("authenticate")]
    public IActionResult PostAuthenticate([FromBody] PostAuthenticateInput loginInput)
    {
        // TODO: Should be removed before production!
        if (_webHostEnvironment.IsDevelopment() && loginInput.Token.Length < 32)
        {
            return DebugLogin(loginInput.Token);
        }
        // ------------------------------------------

        User? user = _userRepositoryService.GetByAuthenticationToken(loginInput.Token);
        if (user == null)
        {
            return Unauthorized();
        }
        if (user.AuthenticationTokenExpiryTime < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        user.AuthenticationToken = null;
        user.AuthenticationTokenExpiryTime = null;
        _userRepositoryService.Update(user);

        Response.Cookies.AppendJwtCookie(_jwtService, user);
        return Ok();
    }

    private IActionResult DebugLogin(string telegramId)
    {
        User? user = _userRepositoryService.GetByTelegramId(telegramId);
        if (user == null)
        {
            return Unauthorized();
        }

        Response.Cookies.AppendJwtCookie(_jwtService, user);
        return Ok();
    }

    private BadRequestObjectResult? IsBotTokenValid(IHeaderDictionary headers)
    {
        StringValues botToken;
        bool botTokenRetrieved = headers.TryGetValue(_botOptions.AuthorizationHeaderName, out botToken);
        if (botTokenRetrieved && botToken == _botOptions.Token)
        {
            return null;
        }
        else
        {
            ErrorResult error = new ErrorResult
            {
                Title = "Invalid bot token",
                Code = ErrorCode.InvalidBotToken,
                Description = "Bot token is invalid, is client bot at all?"
            };

            return BadRequest(error);
        }
    }

    private User? CreateNewUser(PostUsersInput usersInput)
    {
        bool validGuild = Enum.TryParse(usersInput.Guild, out GuildName guildName);
        Guild? guild = _guildRepositoryService.GetByName(guildName);
        if (!validGuild || guild == null)
        {
            return null;
        }

        Coordinate spawnPoint = _spawnGeneratorService.GetSpawnPoint(guildName);

        User user = new()
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

        return user;
    }

    private string CreateNewLoginTokenForUser(User user)
    {
        ReadOnlySpan<char> characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string token = RandomNumberGenerator.GetString(characters, _loginTokenLength);

        user.AuthenticationToken = token;
        user.AuthenticationTokenExpiryTime = DateTime.UtcNow + _loginTokenExpiryTime;
        _userRepositoryService.Update(user);

        return token;
    }
}