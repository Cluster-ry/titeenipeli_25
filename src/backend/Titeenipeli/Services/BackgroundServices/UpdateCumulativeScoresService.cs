using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.Options;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdateCumulativeScoresService : IAsynchronousTimedBackgroundService;

public class UpdateCumulativeScoresService(
    GameOptions gameOptions,
    ApiDbContext dbContext,
    IUserRepositoryService userRepositoryService,
    IGuildRepositoryService guildRepositoryService,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService) : IUpdateCumulativeScoresService
{
    public async Task DoWork()
    {
        var guilds = guildRepositoryService.GetAll();
        foreach (var guild in guilds)
        {
            guild.CurrentScore = 0;
        }

        Pixel[] pixels = await dbContext.Map
                                        .Include(pixel => pixel.User)
                                        .ThenInclude(user => user!.Guild).ToArrayAsync();

        foreach (Pixel pixel in pixels)
            if (pixel.User is { Guild: not null })
            {
                pixel.User.Guild.CurrentScore++;
                pixel.User.Guild.CumulativeScore++;
            }

        await dbContext.SaveChangesAsync();

        UpdateRealtimeScores(guilds);
    }

    private void UpdateRealtimeScores(List<Guild> guilds)
    {
        foreach (var guild in guilds)
        {
            User[] guildUsers = userRepositoryService.GetByGuild(guild.Name);
            foreach (User user in guildUsers)
            {
                GrpcMiscGameStateUpdateInput stateUpdate = new()
                {
                    User = user,
                    MaximumPixelBucket = gameOptions.MaximumPixelBucket,
                    Guilds = guilds
                };
                miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
            }
        }
    }
}