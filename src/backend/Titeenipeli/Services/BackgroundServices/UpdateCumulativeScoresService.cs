using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.BackgroundServices;

public interface IUpdateCumulativeScoresService : IAsynchronousTimedBackgroundService;

public class UpdateCumulativeScoresService(ApiDbContext dbContext) : IUpdateCumulativeScoresService
{
    public async Task DoWork()
    {
        Pixel[] pixels = await dbContext.Map
                                        .Include(pixel => pixel.User)
                                        .ThenInclude(user => user!.Guild).ToArrayAsync();

        foreach (Pixel pixel in pixels)
            if (pixel.User is { Guild: not null })
            {
                pixel.User.Guild.CumulativeScore++;
            }

        await dbContext.SaveChangesAsync();
    }
}