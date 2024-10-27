using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Database.Services.Interfaces;

public interface IMapRepositoryService : IEntityRepositoryService<Pixel>
{
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate);
    public void Update(Pixel pixel);
    public bool IsValid(Coordinate pixelCoordinate);
    public bool IsSpawn(Coordinate pixelCoordinate);
}