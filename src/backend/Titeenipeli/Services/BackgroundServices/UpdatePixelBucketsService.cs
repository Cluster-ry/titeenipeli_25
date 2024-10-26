using Titeenipeli.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdatePixelBucketsService : IAsynchronousTimedBackgroundService;

public class UpdatePixelBucketsService(GameOptions gameOptions, IUserRepositoryService userRepositoryService, IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService) : IUpdatePixelBucketsService
{
    public async Task DoWork()
    {
        foreach (GuildName guildName in Enum.GetValues(typeof(GuildName)))
        {
            UpdateGuildBuckets(guildName);
        }

        await Task.CompletedTask;
    }

    private void UpdateGuildBuckets(GuildName guildName)
    {
        User[] guildUsers = userRepositoryService.GetByGuild(guildName);
        if (guildUsers.Length == 0)
        {
            return;
        }

        float guildPerPlayerIncrease = gameOptions.PixelsPerMinutePerGuild / guildUsers.Length;
        foreach (User user in guildUsers)
        {
            float newBucket = user.PixelBucket + guildPerPlayerIncrease;
            if (newBucket < gameOptions.MaximumPixelBucket)
            {
                user.PixelBucket = newBucket;
            }
            else
            {
                user.PixelBucket = gameOptions.MaximumPixelBucket;
            }
            userRepositoryService.Update(user);

            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = user
            };
            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }
}