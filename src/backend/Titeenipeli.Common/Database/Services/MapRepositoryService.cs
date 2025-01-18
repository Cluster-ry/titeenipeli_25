using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Database.Services;

public class MapRepositoryService(ApiDbContext dbContext) : EntityRepositoryService(dbContext), IMapRepositoryService
{
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate)
    {
        return DbContext.Map.Include(pixel => pixel.User)
                        .ThenInclude(pixelOwner => pixelOwner!.Guild)
                        .FirstOrDefault(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);
    }

    public Pixel? GetById(int id)
    {
        return DbContext.Map.FirstOrDefault(pixel => pixel.Id == id);
    }

    public List<Pixel> GetAll()
    {
        return DbContext.Map
                        .Include(pixel => pixel.User)
                        .ThenInclude(pixelOwner => pixelOwner!.Guild)
                        .OrderBy(pixel => pixel.Y).ToList();
    }

    public void Add(Pixel pixel)
    {
        DbContext.Map.Add(pixel);
    }

    public void Update(Pixel pixel)
    {
        var existingPixel = GetByCoordinate(new Coordinate
        {
            X = pixel.X,
            Y = pixel.Y
        });

        if (existingPixel == null)
        {
            throw new Exception("Pixel doesn't exist.");
        }

        existingPixel.User = pixel.User;
        DbContext.Update(existingPixel);
    }

    public bool IsValid(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return pixel != null;
    }

    public bool IsSpawn(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return !(pixel?.User == null || (pixel.User.SpawnX != pixel.X && pixel.User.SpawnY != pixel.Y));
    }
}