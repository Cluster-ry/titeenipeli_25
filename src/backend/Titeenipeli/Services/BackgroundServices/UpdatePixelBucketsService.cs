using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Options;
using Titeenipeli.Services.Grpc;

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
            await UpdateGuildBuckets(guildName);
        }

        await Task.CompletedTask;
    }

    private async Task UpdateGuildBuckets(GuildName guildName)
    {
        var guildUsers = userRepositoryService.GetByGuild(guildName);
        if (guildUsers.Length == 0)
        {
            return;
        }

        var guild = guildUsers[0].Guild;
        float guildPerPlayerIncrease = guild.BaseRateLimit / guildUsers.Length;
        guild.RateLimitPerPlayer = guildPerPlayerIncrease;
        guildRepositoryService.Update(guild);
        await guildRepositoryService.SaveChangesAsync();

        foreach (var user in guildUsers)
        {
            float newBucket = user.PixelBucket + guildPerPlayerIncrease;
            if (newBucket < guild.PixelBucketSize)
            {
                user.PixelBucket = newBucket;
            }
            else
            {
                user.PixelBucket = guild.PixelBucketSize;
            }
            userRepositoryService.Update(user);
            await userRepositoryService.SaveChangesAsync();

            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = user,
                MaximumPixelBucket = user.Guild.PixelBucketSize,
            };
            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }
}