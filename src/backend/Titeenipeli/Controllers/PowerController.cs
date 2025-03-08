using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Inputs;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/powerups")]
public sealed class PowerController(
    IUserRepositoryService userRepositoryService,
    IMapUpdaterService mapUpdaterService,
    IPowerupService powerupService,
    IJwtService jwtService,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService
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

        if (userPower.Directed && body.Direction is Direction.Undefined)
        {
            return BadRequest();
        }

        var direction = body.Direction;
        if (!userPower.Directed)
        {
            direction = Direction.Undefined;
        }

        var pixelsToPlace = specialEffect.HandleSpecialEffect(new Coordinate(user.SpawnX + body.Location.X, user.SpawnY + body.Location.Y), direction);
        await mapUpdaterService.PlacePixels(userRepositoryService, pixelsToPlace, user);

        user.PowerUps.Remove(userPower);
        userRepositoryService.Update(user);
        await userRepositoryService.SaveChangesAsync();

        SendPowerupMessage(user, userPower);
        SendPowerupUpdate(user);

        return Ok();
    }

    private void SendPowerupUpdate(User user)
    {
        GrpcMiscGameStateUpdateInput stateUpdate = new()
        {
            User = user,
            PowerUps = [.. user.PowerUps]
        };

        miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
    }


    private void SendPowerupMessage(User user, PowerUp powerup)
    {
        foreach (var sendUser in userRepositoryService.GetAll())
        {
            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = sendUser,
                Message = SelectPowerupMessage(user, powerup)
            };

            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }

    private string SelectPowerupMessage(User user, PowerUp powerup)
    {
        string[] messages = new[]
        {
            $"Holy moly, {user.Guild.Name} just used {powerup.Name}!",
            $"{user.Guild.Name} activated {powerup.Name}!",
            $"{user.Guild.Name} just went Super Saiyan with {powerup.Name}!",
            $"{user.Guild.Name} just pulled a 360 no-scope with {powerup.Name}!",
            $"{user.Guild.Name} activated {powerup.Name}!",
            $"{user.Guild.Name} just unleashed {powerup.Name}!",
            $"{user.Guild.Name} used {powerup.Name}!",
            $"{user.Guild.Name} just summoned {powerup.Name}!",
            $"{user.Guild.Name} deployed {powerup.Name}!",
            $"{user.Guild.Name} just destroyed the competition with {powerup.Name}!",
        };

        return Random.Shared.GetItems(messages, 1)[0];
    }
}