using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Options;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdateAvailablePowerupService : IAsynchronousTimedBackgroundService;

public sealed class UpdateAvailablePowerupService(
    IGuildRepositoryService guildRepositoryService,
    IUserRepositoryService userRepositoryService,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService
    ) : IUpdateAvailablePowerupService
{
    public Task DoWork()
    {
        var guilds = guildRepositoryService.GetAll();

        foreach (var guild in guilds)
        {
            var guildUsers = userRepositoryService.GetByGuild(guild.Name);
            foreach (var user in guildUsers)
            {
                UpdateUserPowerups(user);

            }
        }
        return Task.CompletedTask;
    }

    private void UpdateUserPowerups(User user)
    {
        GrpcMiscGameStateUpdateInput stateUpdate = new()
        {
            User = user,
            PowerUps = user.PowerUps.ToList()
        };

        miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
    }
}