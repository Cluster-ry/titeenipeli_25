using Titeenipeli.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Extensions;
using Titeenipeli.Options;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("powers")]
[Authorize(Policy = "MustHaveGuild")]
public sealed class PowerController : ControllerBase
{
    private readonly IMapRepositoryService _mapRepositoryService;
    private readonly IUserRepositoryService _userRepositoryService;
    private readonly IMapUpdaterService _mapUpdaterService;
    private readonly IPowerupRepositoryService _powerupRepositoryService;
    private readonly JwtService _jwtServices;
    private readonly GameOptions _gameOptions;

    private readonly IReadOnlyDictionary<string, Func<User, PowerInput, IActionResult>> powers;
    public PowerController(IMapRepositoryService mapRepositoryService,
                            IUserRepositoryService userRepositoryService,
                            IMapUpdaterService mapUpdaterService,
                            IPowerupRepositoryService powerupService,
                            JwtService jwtService,
                            GameOptions gameOptions
                            )
    {
        _mapRepositoryService = mapRepositoryService;
        _userRepositoryService = userRepositoryService;
        _mapUpdaterService = mapUpdaterService;
        _powerupRepositoryService = powerupService;
        _jwtServices = jwtService;
        _gameOptions = gameOptions;

        powers = new Dictionary<string, Func<User, PowerInput, IActionResult>>(){
            {"Titeenikirves", HandleTiteenikirves}
        };

        AddSupportedPowersToDatabase();
    }

    [HttpPost("Activate")]
    public IActionResult ActivatePower([FromBody] PowerInput body)
    {
        var user = HttpContext.GetUser(_jwtServices, _userRepositoryService);
        if (user is null) return Unauthorized();

        var userPower = user.Powerups.FirstOrDefault(power => power.Id == body.Id);
        if (userPower is null) return Unauthorized();

        powers.TryGetValue(userPower.Name, out var handler);
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

        if (body.direction is Direction.Undefined) return BadRequest();
        else if (body.direction is Direction.North or Direction.South)
        {
            for (var y = 0; y < _gameOptions.Height; y++)
            {
                //Axe cut is 3 pixel wide
                _mapUpdaterService.PlacePixel(new() { X = realX - 1, Y = y }, user);
                _mapUpdaterService.PlacePixel(new() { X = realX, Y = y }, user);
                _mapUpdaterService.PlacePixel(new() { X = realX + 1, Y = y }, user);
            }
        }
        else if (body.direction is Direction.West or Direction.East)
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


    private void AddSupportedPowersToDatabase()
    {
        foreach (var name in powers.Keys)
        {
            _powerupRepositoryService.Add(new PowerUp() { Name = name });
        }
    }
}