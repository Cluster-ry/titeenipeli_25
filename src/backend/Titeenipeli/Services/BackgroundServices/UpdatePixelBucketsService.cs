using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Grpc.Services;
using Titeenipeli.Options;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdatePixelBucketsService : IAsynchronousTimedBackgroundService;

public class UpdatePixelBucketsService(
        GameOptions gameOptions,
        IUserRepositoryService userRepositoryService,
        IGuildRepositoryService guildRepositoryService,
        IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService
    ) : IUpdatePixelBucketsService
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

        var guild = guildUsers[0].Guild;
        float guildPerPlayerIncrease = gameOptions.PixelsPerMinutePerGuild / guildUsers.Length;
        guild.CurrentRateLimitIncreasePerMinutePerPlayer = guildPerPlayerIncrease;
        guildRepositoryService.Update(guild);

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