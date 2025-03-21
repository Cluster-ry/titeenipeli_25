using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Inputs;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;
using PowerUp = Titeenipeli.Common.Database.Schema.PowerUp;

namespace Titeenipeli.Controllers;

[ApiController]
[Authorize]
[Route("ctf")]
public class CtfController : ControllerBase
{
    private readonly ICtfFlagRepositoryService _ctfFlagRepositoryService;
    private readonly IGuildRepositoryService _guildRepositoryService;
    private readonly IUserProvider _userProvider;
    private readonly IJwtService _jwtService;
    private readonly IMiscGameStateUpdateCoreService _miscGameStateUpdateCoreService;
    private readonly ILogger<CtfController> _logger;

    public CtfController(ICtfFlagRepositoryService ctfFlagRepositoryService,
                         IGuildRepositoryService guildRepositoryService,
                         IUserProvider userProvider,
                         IJwtService jwtService,
                         IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService,
                         ILogger<CtfController> logger)
    {
        _ctfFlagRepositoryService = ctfFlagRepositoryService;
        _guildRepositoryService = guildRepositoryService;
        _userProvider = userProvider;
        _jwtService = jwtService;
        _miscGameStateUpdateCoreService = miscGameStateUpdateCoreService;
        _logger = logger;

    }

    [HttpGet]
    public IActionResult GetCtf()
    {
        return Ok("#GOOD_FOR_YOU");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostCtf([FromBody] PostCtfInput ctfInput)
    {
        var user = HttpContext.GetUser(_jwtService, _userProvider);
        var guild = _guildRepositoryService.GetById(user.Guild.Id)!;

        _logger.LogInformation("HTTP POST /ctf user:{user} guild:{guild} flag:{flag} timestamp:{datetime}", user.Id, guild.Id, ctfInput.Token, DateTime.Now);

        var ctfFlag = _ctfFlagRepositoryService.GetByToken(ctfInput.Token);

        if (ctfFlag is null) return BadRequest(new ErrorResult
        {
            Title = "Invalid flag",
            Code = ErrorCode.InvalidCtfFlag,
            Description = "Better luck next time"
        });


        List<CtfFlag> activeCtfFlags;
        if (guild.ActiveCtfFlags != null)
        {
            activeCtfFlags = JsonSerializer.Deserialize<List<CtfFlag>>(guild.ActiveCtfFlags) ?? [];
        }
        else
        {
            activeCtfFlags = [];
        }

        var match = activeCtfFlags?.FirstOrDefault(flag => flag.Id == ctfFlag.Id);
        if (match is not null) return BadRequest(new ErrorResult
        {
            Title = "Flag already used",
            Code = ErrorCode.InvalidCtfFlag,
            Description = "Good try"
        });

        activeCtfFlags?.Add(ctfFlag);
        var newFlags = JsonSerializer.Serialize(activeCtfFlags);
        guild.ActiveCtfFlags = newFlags;
        _guildRepositoryService.Update(guild);
        await _guildRepositoryService.SaveChangesAsync();

        if (ctfFlag.Powerup is null)
        {
            await HandleGuildPowerUp(user, ctfFlag);
        }
        else
        {
            HandleUserPowerUp(user, ctfFlag.Powerup);
        }


        return Ok(CreateCtfResult(ctfFlag));
    }

    private PostCtfResult CreateCtfResult(CtfFlag flag)
    {
        return new PostCtfResult()
        {
            Title = "Flag captured",
            Message = GetGoodEndingMessage(),
            Benefits = GetBenefits(flag),
        };
    }

    private static string GetGoodEndingMessage()
    {
        string[] messages =
        [
            "You nailed it like a last-minute assignment!",
            "Flag captured! Time to celebrate with ramen and Netflix.",
            "Great job! You deserve a break... or maybe just a coffee.",
            "You did it! Now go binge-watch your favorite series.",
            "Victory! Just like acing that surprise quiz.",
            "Well done! Treat yourself to some instant noodles.",
            "You captured the flag! Now back to procrastinating.",
            "Awesome! This calls for a pizza party.",
            "You rock! Time to update your social media status.",
            "Flag secured! Now you can finally take that nap.",
            "The cake is a lie, but not this ctf flag!",
            "You did it! You finally got a ctf token right! Hooray! #WeAreProudOfYou",
            "You know there is no prize for this, right? Just bragging rights.",
            "You know that there other stuff to do at titeens'? Like, you know, the game?",
            "You know that you can't actually eat the flag, right?",
            "Titeens' is not responsible for any injuries sustained during the capture of this flag."
        ];
        return Random.Shared.GetItems(messages, 1)[0];
    }

    private static List<String> GetBenefits(CtfFlag flag)
    {
        var benefits = new List<String>();

        if (flag.BaserateMultiplier != 0) benefits.Add($"Base rate limit increased by {flag.BaserateMultiplier}x");
        if (flag.FogOfWarIncrease != 0) benefits.Add($"Field of view range increased by {flag.FogOfWarIncrease}");
        if (flag.BucketSizeIncrease != 0) benefits.Add($"Pixel bucket size increased by {flag.BucketSizeIncrease}");
        if (flag.Powerup != null)
        {
            benefits.Add($"You got {flag.Powerup.Name}!\n");
        }

        return benefits;
    }

    private async Task HandleGuildPowerUp(User user, CtfFlag ctfFlag)
    {
        var guild = user.Guild;
        if (ctfFlag.BaserateMultiplier != 0) guild.BaseRateLimit *= ctfFlag.BaserateMultiplier;
        if (ctfFlag.FogOfWarIncrease != 0) guild.FogOfWarDistance += ctfFlag.FogOfWarIncrease;
        if (ctfFlag.BucketSizeIncrease != 0) guild.PixelBucketSize += ctfFlag.BucketSizeIncrease;

        _guildRepositoryService.Update(guild);
        await _guildRepositoryService.SaveChangesAsync();
        SendGameUpdate(user);
    }

    private void HandleUserPowerUp(User user, PowerUp powerUp)
    {
        user.PowerUps.Add(powerUp);
        _userProvider.Update(user);

        SendGameUpdate(user);
    }

    private void SendGameUpdate(User user)
    {
        GrpcMiscGameStateUpdateInput stateUpdate = new()
        {
            User = user,
            MaximumPixelBucket = user.Guild.PixelBucketSize,
            PowerUps = user.PowerUps.ToList()
        };

        _miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
    }
}
