using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IMapRepositoryService : IEntityRepositoryService<Pixel>
{
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate);
    public void Update(Pixel pixel);
    public bool IsSpawn(Coordinate pixelCoordinate);
}