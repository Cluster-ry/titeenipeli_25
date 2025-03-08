using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.Extensions;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Inputs;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Controllers;

[ApiController]
[Route("state/powerups")]
public sealed class PowerController(
    IUserProvider userProvider,
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
        var user = HttpContext.GetUser(jwtService, userProvider);

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

        var pixelsToPlace = specialEffect.HandleSpecialEffect(new Coordinate(user.SpawnX + body.Location.X, user.SpawnY + body.Location.Y), body.Direction);
        await mapUpdaterService.PlacePixels(pixelsToPlace, user);

        user.PowerUps.Remove(userPower);
        userProvider.Update(user);

        SendPowerUpMessage(user, userPower);
        SendPowerUpUpdate(user);

        return Ok();
    }

    private void SendPowerUpUpdate(User user)
    {
        GrpcMiscGameStateUpdateInput stateUpdate = new()
        {
            User = user,
            PowerUps = [.. user.PowerUps]
        };

        miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
    }


    private void SendPowerUpMessage(User user, PowerUp powerUp)
    {
        foreach (var sendUser in userProvider.GetAll())
        {
            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = sendUser,
                Message = SelectPowerupMessage(user, powerUp)
            };

            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }

    private static string SelectPowerupMessage(User user, PowerUp powerup)
    {
        string[] messages =
        [
            $"Holy moly, {user.Guild.Name} just used {powerup.Name}!",
            $"{user.Guild.Name} activated {powerup.Name}!",
            $"{user.Guild.Name} just went Super Saiyan with {powerup.Name}!",
            $"{user.Guild.Name} just pulled a 360 no-scope with {powerup.Name}!",
            $"{user.Guild.Name} activated {powerup.Name}!",
            $"{user.Guild.Name} just unleashed {powerup.Name}!",
            $"{user.Guild.Name} used {powerup.Name}!",
            $"{user.Guild.Name} just summoned {powerup.Name}!",
            $"{user.Guild.Name} deployed {powerup.Name}!",
            $"{user.Guild.Name} just destroyed the competition with {powerup.Name}!"
        ];

        return Random.Shared.GetItems(messages, 1)[0];
    }
}