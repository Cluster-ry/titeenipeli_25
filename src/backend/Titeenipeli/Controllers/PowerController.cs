using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/powerups")]
[Authorize(Policy = "MustHaveGuild")]
public sealed class PowerController(
    IUserRepositoryService userRepositoryService,
    IMapUpdaterService mapUpdaterService,
    IPowerupService powerupService,
    IJwtService jwtService
) : ControllerBase
{
    [HttpPost("activate")]
    [Authorize]
    public async Task<IActionResult> ActivatePower([FromBody] PowerInput body)
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);

        var userPower = user.PowerUps.FirstOrDefault(power => power.PowerId == body.Id);
        if (userPower is null)
        {
            return BadRequest();
        }

        var specialEffect = powerupService.GetByDb(userPower);
        if (specialEffect is null)
        {
            return BadRequest();
        }
        
        if(userPower.Directed && body.Direction is Direction.Undefined)
        {
            return BadRequest();
        }

        var pixelsToPlace = specialEffect.HandleSpecialEffect(new Coordinate(body.Location.X, body.Location.Y), body.Direction);
        await mapUpdaterService.PlacePixels(userRepositoryService, pixelsToPlace, user);

        user.PowerUps.Remove(userPower);
        userRepositoryService.Update(user);
        await userRepositoryService.SaveChangesAsync();
        return Ok();
    }
}