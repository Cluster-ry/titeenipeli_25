using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IMapService : IEntityService<Pixel>
{
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate);
    public void Update(Pixel pixel);
    public bool IsValidPlacement(Coordinate pixelCoordinate, User user);
}