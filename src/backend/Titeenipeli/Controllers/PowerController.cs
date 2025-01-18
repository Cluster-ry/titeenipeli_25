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
    [Authorize]
    public async Task<IActionResult> ActivatePower([FromBody] PowerInput body)
    {
        var user = HttpContext.GetUser(jwtService, userRepositoryService);

        var userPower = user.PowerUps.FirstOrDefault(power => power.PowerId == body.Id);
        if (userPower is null)
        {
            return BadRequest();
        }

        Enum.TryParse<Powerups>(userPower.Name, out var powerUp);
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
        await userRepositoryService.SaveChangesAsync();
        return result;
    }

    private IActionResult HandleTiteenikirves(User user, PowerInput body)
    {
        Coordinate realLocation = new Coordinate()
        {
            X = body.Location.X + user.SpawnX,
            Y = body.Location.Y + user.SpawnY
        };

        switch (body.Direction)
        {
            case Direction.Undefined:
                return BadRequest();
            case Direction.North or Direction.South:
                for (int y = 0; y < gameOptions.Height; y++)
                {
                    //Axe cut is 3 pixel wide
                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realLocation.X - 1, Y = y }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realLocation.X, Y = y }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = realLocation.X + 1, Y = y }, user);
                }

                break;
            case Direction.West or Direction.East:
                for (int x = 0; x < gameOptions.Width; x++)
                {
                    //Axe cut is 3 pixel wide
                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realLocation.Y - 1 }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realLocation.Y }, user);

                    mapUpdaterService.PlacePixel(
                        mapRepositoryService,
                        userRepositoryService,
                        new Coordinate { X = x, Y = realLocation.Y + 1 }, user);
                }

                break;
        }

        return Ok();
    }

    private Func<User, PowerInput, IActionResult>? SelectPowerHandler(Powerups? powerUp)
    {
        return powerUp switch
        {
            Powerups.Titeenikirves => HandleTiteenikirves,
            _ => null
        };
    }

    public static string GetDescription(Powerups powerup) => powerup switch
    {
        Powerups.Titeenikirves => "Take might into your own hands and split the battlefield in half with a mighty 3 pixel wide titeenikirves.",
        _ => throw new NotSupportedException($"{nameof(Powerups)} with value {powerup.ToString()} = {(int)powerup} is not supported.")
    };
}