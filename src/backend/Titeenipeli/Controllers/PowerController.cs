using Titeenipeli.Services.RepositoryServices.Interfaces;
using Titeenipeli.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Schema;
using Titeenipeli.Extensions;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Identity;
using Titeenipeli.Models;
using Titeenipeli.Options;

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

    public PowerController(IMapRepositoryService mapRepositoryService,
                            IUserRepositoryService userRepositoryService,
                            IMapUpdaterService mapUpdaterService,
                            JwtService jwtService,
                            GameOptions gameOptions)
    {
        _mapRepositoryService = mapRepositoryService;
        _userRepositoryService = userRepositoryService;
        _mapUpdaterService = mapUpdaterService;
        _jwtServices = jwtService;
        _gameOptions = gameOptions;
    }

    [HttpPost("Activate")]
    public IActionResult ActivatePower([FromBody] PowerInput body)
    {
        var jwtClaim = HttpContext.GetUser(_jwtServices);
        if (jwtClaim is null) return Unauthorized();

        var powers = _powerupRepositoryService.UserPowers(jwtClaim.Id);
        var power = powers?.FirstOrDefault(power => power.Id == body.Id);
        if (power is null) return Unauthorized();

        switch (power.Name)
        {
            case "Titeenikirves":
                HandleTiteenikirves(jwtClaim.Id, body);
                break;
            default:
                break;
        }

        return Ok();
    }

    private void HandleTiteenikirves(int userId, PowerInput body)
    {
        var user = _userRepositoryService.GetById(userId);
        var realX = user.SpawnX + body.Location.X;
        var realY = user.SpawnY + body.Location.Y;

        if (body.direction is Direction.North or Direction.South)
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
    }
}