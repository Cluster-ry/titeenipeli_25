using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Inputs;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;
using PowerUp = Titeenipeli.Common.Database.Schema.PowerUp;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
public class CtfController : ControllerBase
{
    private readonly ICtfFlagRepositoryService _ctfFlagRepositoryService;
    private readonly IGuildRepositoryService _guildRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly IJwtService _jwtService;
    private readonly IMiscGameStateUpdateCoreService _miscGameStateUpdateCoreService;

    public CtfController(ICtfFlagRepositoryService ctfFlagRepositoryService, IUserRepositoryService userRepositoryService, IGuildRepositoryService guildRepositoryService, IJwtService jwtService, IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService)
    {
        _ctfFlagRepositoryService = ctfFlagRepositoryService;
        _userRepositoryService = userRepositoryService;
        _guildRepositoryService = guildRepositoryService;
        _jwtService = jwtService;
        _miscGameStateUpdateCoreService = miscGameStateUpdateCoreService;

    }

    [HttpPost("ctf")]
    [Authorize]
    public async Task<IActionResult> PostCtf([FromBody] PostCtfInput ctfInput)
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
        await _guildRepositoryService.SaveChangesAsync();

        if (ctfFlag.Powerup is null)
        {
            await HandleGuildPowerUp(user.Guild, ctfFlag);
        }
        else
        {
            await HandleUserPowerUp(user, ctfFlag.Powerup);
        }

        return Ok();
    }


    private async Task HandleGuildPowerUp(Guild guild, CtfFlag ctfFlag)
    {
        if (ctfFlag.BaserateMultiplier != 0) guild.BaseRateLimit *= ctfFlag.BaserateMultiplier;
        if (ctfFlag.FovRangeIncrease != 0) guild.FovRangeDistance += ctfFlag.FovRangeIncrease;

        _guildRepositoryService.Update(guild);
        await _guildRepositoryService.SaveChangesAsync();
    }

    private async Task HandleUserPowerUp(User user, PowerUp powerUp)
    {
        user.PowerUps.Add(powerUp);
        _userRepositoryService.Update(user);
        await _userRepositoryService.SaveChangesAsync();

        GrpcMiscGameStateUpdateInput stateUpdate = new()
        {
            User = user,
            PowerUps = user.PowerUps.ToList()
        };

        _miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
    }
}
