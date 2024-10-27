using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Database.Services;

public class MapRepositoryService : IMapRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public MapRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Pixel? GetByCoordinate(Coordinate pixelCoordinate)
    {
        return _dbContext.Map.Include(pixel => pixel.User)
                         .ThenInclude(pixelOwner => pixelOwner!.Guild)
                         .FirstOrDefault(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);
    }

    public Pixel? GetById(int id)
    {
        return _dbContext.Map.FirstOrDefault(pixel => pixel.Id == id);
    }

    public List<Pixel> GetAll()
    {
        return _dbContext.Map
                         .Include(pixel => pixel.User)
                         .ThenInclude(pixelOwner => pixelOwner!.Guild)
                         .OrderBy(pixel => pixel.Y).ToList();
    }

    public void Add(Pixel pixel)
    {
        _dbContext.Map.Add(pixel);
        _dbContext.SaveChanges();
    }

    public void Update(Pixel pixel)
    {
        Pixel? existingPixel = GetByCoordinate(new Coordinate
        {
            X = pixel.X,
            Y = pixel.Y
        });

        if (existingPixel == null)
        {
            throw new Exception("Pixel doesn't exist.");
        }

        existingPixel.User = pixel.User;
        _dbContext.Update(existingPixel);
        _dbContext.SaveChanges();
    }

    public bool IsValid(Coordinate pixelCoordinate)
    {
        Pixel? pixel = GetByCoordinate(pixelCoordinate);
        return pixel != null;
    }

    public bool IsSpawn(Coordinate pixelCoordinate)
    {
        Pixel? pixel = GetByCoordinate(pixelCoordinate);
        return !(pixel?.User == null || (pixel.User.SpawnX != pixel.X && pixel.User.SpawnY != pixel.Y));
    }
}