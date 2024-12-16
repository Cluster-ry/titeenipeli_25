using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/powerups")]
[Authorize(Policy = "MustHaveGuild")]
public sealed class PowerController(
    IUserRepositoryService userRepositoryService,
    IMapRepositoryService mapRepositoryService,
    IMapUpdaterService mapUpdaterService,
    IJwtService jwtService,
    GameOptions gameOptions
) : ControllerBase
{
    [HttpPost("activate")]
    [Authorize(Policy = "MustHaveGuild")]
    public IActionResult ActivatePower([FromBody] PowerInput body)
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);

        var userPower = user.PowerUps.FirstOrDefault(power => power.Id == body.Id);
        if (userPower is null)
        {
            return BadRequest();
        }

        Enum.TryParse<PowerUps>(userPower.Name, out var powerUp);
        var handler = SelectPowerHandler(powerUp);
        if (handler is null)
        {
            return BadRequest();
        }

        var result = handler(user, body);

        if (result is not OkObjectResult)
        {
            return result;
        }

        user.PowerUps.Remove(userPower);
        userRepositoryService.Update(user);
        return result;
    }

    private IActionResult HandleTiteenikirves(User user, PowerInput body)
    {
        int realX = user.SpawnX + body.Location.X;
        int realY = user.SpawnY + body.Location.Y;

        switch (body.Direction)
        {
            case DirectionEnum.Undefined:
                return BadRequest();
            case DirectionEnum.North or DirectionEnum.South:
                for (int y = 0; y < gameOptions.Height; y++)
                {
                    //Axe cut is 3 pixel wide
                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realX - 1, Y = y }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realX, Y = y }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realX + 1, Y = y }, user);
                }

                break;
            case DirectionEnum.West or DirectionEnum.East:
                for (int x = 0; x < gameOptions.Width; x++)
                {
                    //Axe cut is 3 pixel wide
                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realY - 1 }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realY }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realY + 1 }, user);
                }

                break;
        }

        return Ok();
    }

    private Func<User, PowerInput, IActionResult>? SelectPowerHandler(PowerUps? powerUp)
    {
        return powerUp switch
        {
            PowerUps.Titeenikirves => HandleTiteenikirves,
            _ => null
        };
    }
}

public enum PowerUps
{
    Titeenikirves
}