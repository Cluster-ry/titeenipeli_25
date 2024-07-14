using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Schema;

namespace Titeenipeli.BackgroundServices;

public interface IUpdateCumulativeScoresService : IAsynchronousTimedBackgroundService;

public class UpdateCumulativeScoresService(ApiDbContext dbContext) : IUpdateCumulativeScoresService
{
    private readonly ApiDbContext _dbContext = dbContext;

    public async Task DoWork()
    {
        Pixel[] pixels = await _dbContext.Map
            .Include(pixel => pixel.User)
            .ThenInclude(user => user!.Guild).ToArrayAsync();

        foreach (Pixel pixel in pixels)
        {
            if (pixel.User != null)
            {
                pixel.User.Guild.CumulativeScore++;
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}