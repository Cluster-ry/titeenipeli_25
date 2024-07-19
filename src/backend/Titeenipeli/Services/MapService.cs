using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Services;

public class MapService : IMapService
{
    private readonly ApiDbContext _dbContext;

    public MapService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Pixel> GetPixels()
    {
        return _dbContext.Map
            .Include(pixel => pixel.User)
            .ThenInclude(pixelOwner => pixelOwner!.Guild)
            .OrderBy(pixel => pixel.Y).ToList();
    }
}