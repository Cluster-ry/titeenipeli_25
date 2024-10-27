using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Database.Services.Interfaces;

public interface IMapRepositoryService : IEntityRepositoryService<Pixel>
{
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate);
    public void Update(Pixel pixel);
    public bool IsValid(Coordinate pixelCoordinate);
    public bool IsSpawn(Coordinate pixelCoordinate);
}