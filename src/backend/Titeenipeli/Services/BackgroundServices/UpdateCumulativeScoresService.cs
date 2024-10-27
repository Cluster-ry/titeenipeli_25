using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdateCumulativeScoresService : IAsynchronousTimedBackgroundService;

public class UpdateCumulativeScoresService(ApiDbContext dbContext, IGuildRepositoryService guildRepositoryService) : IUpdateCumulativeScoresService
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
    }
}