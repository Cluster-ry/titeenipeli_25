using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Grpc.ChangeEntities;
using Titeenipeli.InMemoryProvider.MapProvider;
using Titeenipeli.InMemoryProvider.UserProvider;
using Titeenipeli.Services.Grpc;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdateCumulativeScoresService : IAsynchronousTimedBackgroundService;

public class UpdateCumulativeScoresService(
    IServiceScopeFactory serviceScopeFactory,
    IUserProvider userProvider,
    IMapProvider mapProvider,
    IMiscGameStateUpdateCoreService miscGameStateUpdateCoreService) : IUpdateCumulativeScoresService
{
    public async Task DoWork()
    {
        var scope = serviceScopeFactory.CreateScope();
        var guildRepositoryService = scope.ServiceProvider.GetRequiredService<IGuildRepositoryService>();

        var guilds = guildRepositoryService.GetAll();
        foreach (var guild in guilds)
        {
            guild.CurrentScore = 0;
        }

        Pixel[] pixels = [.. mapProvider.GetAll()];

        foreach (var pixel in pixels)
        {
            if (pixel.User is null)
            {
                continue;
            }

            // This is done to get guild from correct context.
            var pixelGuild = guilds.FirstOrDefault(g => g.Id == pixel.User.Guild.Id);

            pixelGuild!.CurrentScore++;
            pixelGuild.CumulativeScore++;
        }

        await guildRepositoryService.SaveChangesAsync();

        UpdateRealtimeScores(guilds);
    }

    private void UpdateRealtimeScores(List<Guild> guilds)
    {
        foreach (var user in userProvider.GetAll())
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