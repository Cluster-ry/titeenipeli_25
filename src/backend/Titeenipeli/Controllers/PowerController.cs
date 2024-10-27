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
using System.Collections.Generic;

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

    private readonly IReadOnlyDictionary<string, Action<int, PowerInput>> powers;
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

        powers = new Dictionary<string, Action<int, PowerInput>>(){
            {"Titeenikirves", HandleTiteenikirves}
        };
    }

    [HttpPost("Activate")]
    public IActionResult ActivatePower([FromBody] PowerInput body)
    {
        var jwtClaim = HttpContext.GetUser(_jwtServices);
        if (jwtClaim is null) return Unauthorized();

        var userPowers = _powerupRepositoryService.UserPowers(jwtClaim.Id);
        var userPower = userPowers?.FirstOrDefault(power => power.Id == body.Id);
        if (userPower is null) return Unauthorized();

        powers.TryGetValue(userPower.Name, out var handler);
        if(handler is null) return Unauthorized(); //What should return?

        handler(jwtClaim.Id, body);

        return Ok();
    }

    private void HandleTiteenikirves(int userId, PowerInput body)
    {
        var user = _userRepositoryService.GetById(userId);
        if(user is null) return;
        
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


    private void AddSupportedPowersToDatabase(){
        foreach(var name in powers.Keys)
        {
            _powerupRepositoryService.Add(new PowerUp(){Name=name});
        }
    }
}