using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Inputs;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("users")]
public class UserController(
    IHostEnvironment webHostEnvironment,
    BotOptions botOptions,
    GameOptions gameOptions,
    IUserRepositoryService userRepositoryService,
    IGuildRepositoryService guildRepositoryService,
    IJwtService jwtService,
    IMapUpdaterService mapUpdaterService) : ControllerBase
{
    private const int LoginTokenLength = 32;

    private readonly TimeSpan _loginTokenExpiryTime = TimeSpan.FromMinutes(botOptions.LoginTokenExpirationInMinutes);


    [HttpGet("current")]
    [Authorize]
    public IActionResult CurrentUser()
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);

        return Ok(new GetCurrentUserResult(user));
    }

    [HttpPost]
    public async Task<IActionResult> PostUsers([FromBody] PostUsersInput usersInput)
    {
        var botTokenError = IsBotTokenValid(Request.Headers);
        if (botTokenError != null)
        {
            return botTokenError;
        }

        var user = userRepositoryService.GetByTelegramId(usersInput.TelegramId);

        if (user == null)
        {
            user = await CreateNewUser(usersInput);
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

        string token = await CreateNewLoginTokenForUser(user);
        PostUsersResult postUsersResult = new()
        {
            Token = token
        };

        return Ok(postUsersResult);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> PostAuthenticate([FromBody] PostAuthenticateInput loginInput)
    {
        // TODO: Should be removed before production!
        return DebugLogin(loginInput.Token);
        if (webHostEnvironment.IsDevelopment() && loginInput.Token.Length < 32)
        {
        }
        // ------------------------------------------

        var user = userRepositoryService.GetByAuthenticationToken(loginInput.Token);
        if (user == null)
        {
            return Unauthorized();
        }

        if (user.AuthenticationTokenExpiryTime < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        if (!user.IsGod)
        {
            user.AuthenticationToken = null;
            user.AuthenticationTokenExpiryTime = null;
        }

        userRepositoryService.Update(user);
        await userRepositoryService.SaveChangesAsync();

        Response.Cookies.AppendJwtCookie(jwtService, user);
        return Ok();
    }

    private IActionResult DebugLogin(string telegramId)
    {
        var user = userRepositoryService.GetByTelegramId(telegramId);
        if (user == null)
        {
            return Unauthorized();
        }

        Response.Cookies.AppendJwtCookie(jwtService, user);
        return Ok();
    }

    private BadRequestObjectResult? IsBotTokenValid(IHeaderDictionary headers)
    {
        bool botTokenRetrieved = headers.TryGetValue(botOptions.AuthorizationHeaderName, out var botToken);
        if (botTokenRetrieved && botToken == botOptions.Token)
        {
            return null;
        }

        var error = new ErrorResult
        {
            Title = "Invalid bot token",
            Code = ErrorCode.InvalidBotToken,
            Description = "Bot token is invalid, is client bot at all?"
        };

        return BadRequest(error);
    }

    private async Task<User?> CreateNewUser(PostUsersInput usersInput)
    {
        bool validGuild = Enum.TryParse(usersInput.Guild, out GuildName guildName);
        var guild = guildRepositoryService.GetByName(guildName);

        if (!validGuild || guild == null)
        {
            return null;
        }

        User user = new()
        {
            Guild = guild,
            Code = "",

            SpawnX = -1,
            SpawnY = -1,

            PixelBucket = gameOptions.InitialPixelBucket,

            PowerUps = [],

            TelegramId = usersInput.TelegramId,
            FirstName = usersInput.FirstName,
            LastName = usersInput.LastName,
            Username = usersInput.Username
        };

        userRepositoryService.Add(user);
        await userRepositoryService.SaveChangesAsync();

        user = await mapUpdaterService.PlaceSpawn(userRepositoryService, user);
        userRepositoryService.Update(user);
        await userRepositoryService.SaveChangesAsync();

        return user;
    }

    private async Task<string> CreateNewLoginTokenForUser(User user)
    {
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string token = RandomNumberGenerator.GetString(characters, LoginTokenLength);

        user.AuthenticationToken = token;
        user.AuthenticationTokenExpiryTime = DateTime.UtcNow + _loginTokenExpiryTime;
        userRepositoryService.Update(user);
        await userRepositoryService.SaveChangesAsync();

        return token;
    }
}