using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Services;
using Titeenipeli.SpecialEffects;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/powerups")]
[Authorize(Policy = "MustHaveGuild")]
public sealed class PowerController(
    IUserRepositoryService userRepositoryService,
    IMapUpdaterService mapUpdaterService,
    IJwtService jwtService,
    GameOptions gameOptions
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

        Enum.TryParse<PowerUps>(userPower.Name, out var powerUp);
        var specialEffect = SelectSpecialEffect(powerUp);
        if (specialEffect is null)
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



    private ISpecialEffect? SelectSpecialEffect(PowerUps? powerUp)
    {
        return powerUp switch
        {
            PowerUps.TestEffect => new TestEffect(),
            PowerUps.Titeenikirves => new TiteenikirvesEffect(gameOptions.Height, gameOptions.Width),
            _ => null
        };
    }

    public static string GetDescription(PowerUps powerUp)
    {
        return powerUp switch
        {
            PowerUps.Titeenikirves =>
                "Take might into your own hands and split the battlefield in half with a mighty 3 pixel wide axe of the Titeen's.",
            _ => throw new NotSupportedException(
                $"{nameof(PowerUps)} with value {powerUp.ToString()} = {(int)powerUp} is not supported.")
        };
    }
}