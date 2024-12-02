using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
public class CtfController : ControllerBase
{
    private readonly ICtfFlagRepositoryService _ctfFlagRepositoryService;
    private readonly IGuildRepositoryService _guildRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly IJwtService _jwtService;

    public CtfController(ICtfFlagRepositoryService ctfFlagRepositoryService, IUserRepositoryService userRepositoryService, IGuildRepositoryService guildRepositoryService, IJwtService jwtService)
    {
        _ctfFlagRepositoryService = ctfFlagRepositoryService;
        _userRepositoryService = userRepositoryService;
        _guildRepositoryService = guildRepositoryService;
        _jwtService = jwtService;
    }

    [HttpPost("ctf")]
    public IActionResult PostCtf([FromBody] PostCtfInput ctfInput)
    {
        var ctfFlag = _ctfFlagRepositoryService.GetByToken(ctfInput.Token);

        if (ctfFlag is null) return BadRequest(new ErrorResult
        {
            Title = "Invalid flag",
            Code = ErrorCode.InvalidCtfFlag,
            Description = "Better luck next time"
        });

        var user = HttpContext.GetUser(_jwtService, _userRepositoryService);
        var guild = user.Guild;

        var match = guild.ActiveCtfFlags.FirstOrDefault(flag => flag.Id == ctfFlag.Id);
        if (match is not null) return BadRequest(new ErrorResult
        {
            Title = "Flag already used",
            Code = ErrorCode.InvalidCtfFlag,
            Description = "Good try"
        });

        guild.ActiveCtfFlags.Add(ctfFlag);
        _guildRepositoryService.Update(guild);

        if (ctfFlag.Powerup is null) HandleGuildPowerup(user.Guild);
        else HandleUserPowerup(user, ctfFlag.Powerup);

        return Ok();
    }


    private void HandleGuildPowerup(Guild guild)
    {
        //TODO run powerup code
    }

    private void HandleUserPowerup(User user, PowerUp powerup)
    {
        user.Powerups.Add(powerup);
        _userRepositoryService.Update(user);
    }
}
