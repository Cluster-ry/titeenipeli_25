using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdatePixelBucketsService : IAsynchronousTimedBackgroundService;

public class UpdatePixelBucketsService(
    IUserProvider userProvider,
        IGuildRepositoryService guildRepositoryService,
        IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService
    ) : IUpdatePixelBucketsService
{
    public async Task DoWork()
    {
        foreach (var guildName in Enum.GetValues<GuildName>())
        {
            await UpdateGuildBuckets(guildName);
        }

        await Task.CompletedTask;
    }

    private async Task UpdateGuildBuckets(GuildName guildName)
    {
        var guildUsers = userProvider.GetByGuild(guildName);
        if (guildUsers.Count == 0)
        {
            return;
        }

        var guild = guildUsers[0].Guild;
        float guildPerPlayerIncrease = guild.BaseRateLimit / guildUsers.Count;
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

            userProvider.Update(user);

            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = user,
                MaximumPixelBucket = user.Guild.PixelBucketSize,
            };
            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }
}