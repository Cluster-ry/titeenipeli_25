using Titeenipeli.Enums;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdatePixelBucketsService : IAsynchronousTimedBackgroundService;

public class UpdatePixelBucketsService(GameOptions gameOptions, IUserRepositoryService userRepositoryService) : IUpdatePixelBucketsService
{
    public async Task DoWork()
    {
        foreach (GuildName guildName in Enum.GetValues(typeof(GuildName)))
        {
            User[] guildUsers = userRepositoryService.GetByGuild(guildName);
            UpdateGuildBuckets(guildUsers);
        }

        await Task.CompletedTask;
    }

    private void UpdateGuildBuckets(User[] guildUsers)
    {
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
        }
    }
}