using Microsoft.EntityFrameworkCore;
using Titeenipeli.Context;
using Titeenipeli.Models;
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

    public Pixel? GetPixel(Coordinate pixelCoordinate)
    {
        return _dbContext.Map.FirstOrDefault(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);
    }

    public List<Pixel> GetPixels()
    {
        return _dbContext.Map
            .Include(pixel => pixel.User)
            .ThenInclude(pixelOwner => pixelOwner!.Guild)
            .OrderBy(pixel => pixel.Y).ToList();
    }

    public void AddPixel(Pixel pixel)
    {
        _dbContext.Map.Add(pixel);
        _dbContext.SaveChanges();
    }

    public void UpdatePixel(Pixel pixel)
    {
        Pixel? existingUser = GetPixel(new Coordinate
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

    public bool IsValidPlacement(Coordinate pixelCoordinate, User user)
    {
        // Take neighboring pixels for the pixel the user is trying to set,
        // but remove cornering pixels and only return pixels belonging to
        // the user
        return (from pixel in _dbContext.Map
                where Math.Abs(pixel.X - pixelCoordinate.X) <= 1 &&
                      Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      Math.Abs(pixel.X - pixelCoordinate.X) + Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      pixel.User == user
                select pixel).Any();
    }
}