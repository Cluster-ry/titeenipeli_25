using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Results;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("users")]
public class UserController(
                      IWebHostEnvironment webHostEnvironment,
                      BotOptions botOptions,
                      SpawnGeneratorService spawnGeneratorService,
                      IUserRepositoryService userRepositoryService,
                      IGuildRepositoryService guildRepositoryService,
                      JwtService jwtService,
                      IMapRepositoryService mapRepositoryService) : ControllerBase
{
    private const int _loginTokenLength = 32;

    private readonly TimeSpan _loginTokenExpiryTime = TimeSpan.FromMinutes(botOptions.LoginTokenExpirationInMinutes);


    [HttpGet("current")]
    [Authorize]
    public IActionResult CurrentUser()
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);
        if (user is null) return Unauthorized();

        return Ok(new UserResult(user));
    }

    public class UserResult
    {
        public int Id { get; init; }
        public string Username { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public int Guild { get; init; }


        public UserResult(User user)
        {
            Id = user.Id;
            Username = user.Username;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Guild = user.Guild.Id;
        }
    }

    [HttpPost]
    public IActionResult PostUsers([FromBody] PostUsersInput usersInput)
    {
        BadRequestObjectResult? botTokenError = IsBotTokenValid(Request.Headers);
        if (botTokenError != null)
        {
            return botTokenError;
        }

        User? user = userRepositoryService.GetByTelegramId(usersInput.TelegramId);

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
        if (webHostEnvironment.IsDevelopment() && loginInput.Token.Length < 32)
        {
            return DebugLogin(loginInput.Token);
        }
        // ------------------------------------------

        User? user = userRepositoryService.GetByAuthenticationToken(loginInput.Token);
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
        userRepositoryService.Update(user);

        Response.Cookies.AppendJwtCookie(jwtService, user);
        return Ok();
    }

    private IActionResult DebugLogin(string telegramId)
    {
        User? user = userRepositoryService.GetByTelegramId(telegramId);
        if (user == null)
        {
            return Unauthorized();
        }

        Response.Cookies.AppendJwtCookie(jwtService, user);
        return Ok();
    }

    private BadRequestObjectResult? IsBotTokenValid(IHeaderDictionary headers)
    {
        StringValues botToken;
        bool botTokenRetrieved = headers.TryGetValue(botOptions.AuthorizationHeaderName, out botToken);
        if (botTokenRetrieved && botToken == botOptions.Token)
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
        Guild? guild = guildRepositoryService.GetByName(guildName);
        if (!validGuild || guild == null)
        {
            return null;
        }

        Coordinate spawnPoint = spawnGeneratorService.GetSpawnPoint(guildName);

        User user = new()
        {
            Guild = guild,
            Code = "",

            SpawnX = spawnPoint.X,
            SpawnY = spawnPoint.Y,

            Powerups = new(),

            TelegramId = usersInput.TelegramId,
            FirstName = usersInput.FirstName,
            LastName = usersInput.LastName,
            Username = usersInput.Username,
        };

        userRepositoryService.Add(user);

        Pixel pixel = new()
        {
            X = spawnPoint.X,
            Y = spawnPoint.Y,
            User = user
        };

        mapRepositoryService.Update(pixel);

        return user;
    }

    private string CreateNewLoginTokenForUser(User user)
    {
        ReadOnlySpan<char> characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string token = RandomNumberGenerator.GetString(characters, _loginTokenLength);

        user.AuthenticationToken = token;
        user.AuthenticationTokenExpiryTime = DateTime.UtcNow + _loginTokenExpiryTime;
        userRepositoryService.Update(user);

        return token;
    }
}