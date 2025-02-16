using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state")]
[Authorize]
public class GameStateController(
        GameOptions gameOptions,
        IUserRepositoryService userRepositoryService,
        IGuildRepositoryService guildRepositoryService,
        IPowerupService powerupService,
        IJwtService jwtService
    ) : ControllerBase
{
    [HttpGet("game")]
    public IActionResult GetGameState()
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);
        var guilds = guildRepositoryService.GetAll();

        List<Score> scores = guilds.Select(guild => new Score()
        {
            Guild = guild.Name,
            Amount = guild.CurrentScore
        }).ToList();

        List<PowerUp> powerups = user.PowerUps.Select(power =>
        {
            var info = powerupService.GetByDb(power);
            return new PowerUp()
            {
                PowerUpId = power.PowerId,
                Name = power.Name,
                Description = info!.Description,
                Directed = info.Directed,
            };
        }).ToList();

        GameStateResults results = new()
        {
            PixelBucket = new()
            {
                Amount = (int)user.PixelBucket,
                MaxAmount = user.Guild.PixelBucketSize,
                IncreasePerMinute = user.Guild.RateLimitPerPlayer,
            },
            Scores = scores,
            PowerUps = powerups
        };

        return Ok(results);
    }
}