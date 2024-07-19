using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IMapService
{
    public List<Pixel> GetPixels();
    public bool IsValidPlacement(Coordinate pixelCoordinate, User user);
}