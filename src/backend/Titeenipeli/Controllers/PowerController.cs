using Titeenipeli.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Extensions;
using Titeenipeli.Options;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Inputs;

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
    private readonly IUserRepositoryService _userRepositoryService = userRepositoryService;
    private readonly IMapUpdaterService _mapUpdaterService = mapUpdaterService;
    private readonly IJwtService _jwtServices = jwtService;
    private readonly GameOptions _gameOptions = gameOptions;

    [HttpPost("activate")]
    [Authorize(Policy = "MustHaveGuild")]
    public IActionResult ActivatePower([FromBody] PowerInput body)
    {
        var user = HttpContext.GetUser(_jwtServices, _userRepositoryService);
        if (user is null) return Unauthorized();

        var userPower = user.Powerups.FirstOrDefault(power => power.Id == body.Id);
        if (userPower is null) return Unauthorized();

        Enum.TryParse<Powerups>(userPower.Name, out var powerupEnum);
        var handler = SelectPowerHandler(powerupEnum);
        if (handler is null) return BadRequest();

        var result = handler(user, body);
        if (result is OkObjectResult)
        {
            user.Powerups.Remove(userPower);
            _userRepositoryService.Update(user);
        }
        return result;
    }

    private IActionResult HandleTiteenikirves(User user, PowerInput body)
    {
        var realX = user.SpawnX + body.Location.X;
        var realY = user.SpawnY + body.Location.Y;

        if (body.Direction is DirectionEnum.Undefined) return BadRequest();
        else if (body.Direction is DirectionEnum.North or DirectionEnum.South)
        {
            for (var y = 0; y < _gameOptions.Height; y++)
            {
                //Axe cut is 3 pixel wide
                _mapUpdaterService.PlacePixel(new() { X = realX - 1, Y = y }, user);
                _mapUpdaterService.PlacePixel(new() { X = realX, Y = y }, user);
                _mapUpdaterService.PlacePixel(new() { X = realX + 1, Y = y }, user);
            }
        }
        else if (body.Direction is DirectionEnum.West or DirectionEnum.East)
        {
            for (var x = 0; x < _gameOptions.Width; x++)
            {
                //Axe cut is 3 pixel wide
                _mapUpdaterService.PlacePixel(new() { X = x, Y = realY - 1 }, user);
                _mapUpdaterService.PlacePixel(new() { X = x, Y = realY }, user);
                _mapUpdaterService.PlacePixel(new() { X = x, Y = realY + 1 }, user);
            }
        }

        return Ok();
    }

    private Func<User, PowerInput, IActionResult>? SelectPowerHandler(Powerups? powerup)
    => powerup switch
    {
        Powerups.Titeenikirves => HandleTiteenikirves,
        _ => null,
    };
}

public enum Powerups
{
    Titeenikirves,
}