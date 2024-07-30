using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Models;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.RepositoryServices;

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
        Pixel? existingUser = GetByCoordinate(new Coordinate
        {
            X = pixel.X,
            Y = pixel.Y
        });

        if (existingUser == null)
        {
            throw new Exception("Pixel doesn't exist.");
        }

        _dbContext.Entry(existingUser).CurrentValues.SetValues(pixel);
        _dbContext.SaveChanges();
    }

    public bool IsSpawn(Coordinate pixelCoordinate)
    {
        Pixel? pixel = GetByCoordinate(pixelCoordinate);
        return pixel?.User == null || (pixel.User.SpawnX != pixel.X && pixel.User.SpawnY != pixel.Y);
    }
}