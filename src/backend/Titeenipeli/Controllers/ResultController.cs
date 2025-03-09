using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Results;
using Titeenipeli.Extensions;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("result")]
[Authorize]
public class ResultController(
    IGuildRepositoryService guildRepositoryService,
    IUserProvider userProvider,
    IJwtService jwtService
) : ControllerBase
{
    [HttpGet("score")]
    public IActionResult GetResults()
    {
        var user = HttpContext.GetUser(jwtService, userProvider);

        if (!user.IsGod)
        {
            return Unauthorized();
        }

        var guildScores = new GetScoreResult
        {
            Scores = guildRepositoryService.GetAll().Select(guild => new GuildScore
            {
                Guild = guild.Name,
                Score = guild.CumulativeScore
            }).ToList()
        };

        return Ok(guildScores);
    }
}