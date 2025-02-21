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
    IUserRepositoryService userRepositoryService,
    IGuildRepositoryService guildRepositoryService,
    IMapRepositoryService mapRepositoryService,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService) : IUpdateCumulativeScoresService
{
    public async Task DoWork()
    {
        var guilds = guildRepositoryService.GetAll();
        foreach (var guild in guilds)
        {
            guild.CurrentScore = 0;
        }

        Pixel[] pixels = [.. mapRepositoryService.GetAll()];

        foreach (Pixel pixel in pixels)
            if (pixel.User is { Guild: not null })
            {
                pixel.User.Guild.CurrentScore++;
                pixel.User.Guild.CumulativeScore++;
            }

        await guildRepositoryService.SaveChangesAsync();

        UpdateRealtimeScores(guilds);
    }

    private void UpdateRealtimeScores(List<Guild> guilds)
    {
        foreach (var user in userRepositoryService.GetAll())
        {
            GrpcMiscGameStateUpdateInput stateUpdate = new()
            {
                User = user,
                MaximumPixelBucket = user.Guild.PixelBucketSize,
                Guilds = guilds,
            };
            miscGameStateUpdateCoreService.UpdateMiscGameState(stateUpdate);
        }
    }
}