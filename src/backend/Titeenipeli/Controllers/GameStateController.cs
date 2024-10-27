using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Extensions;
using Titeenipeli.Options;
using Titeenipeli.Results;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state")]
[Authorize]
public class GameStateController(
        GameOptions gameOptions,
        IUserRepositoryService userRepositoryService,
        IGuildRepositoryService guildRepositoryService,
        JwtService jwtService
    ) : ControllerBase
{
    [HttpGet("game")]
    public IActionResult GetGameState()
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);
        var guilds = guildRepositoryService.GetAll();

        List<Score> scores = [];
        foreach (var guild in guilds)
        {
            scores.Add(new()
            {
                Guild = guild.Name,
                Amount = guild.CurrentScore
            });
        }

        GameStateResults results = new()
        {
            PixelBucket = new()
            {
                Amount = (int)user.PixelBucket,
                MaxAmount = gameOptions.MaximumPixelBucket,
                IncreasePerMinute = user.Guild.CurrentRateLimitIncreasePerMinutePerPlayer,
            },
            Scores = scores
        };

        return Ok(results);
    }
}