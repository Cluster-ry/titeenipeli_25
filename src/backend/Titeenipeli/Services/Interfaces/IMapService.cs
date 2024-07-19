using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IMapService
{
    public Pixel? GetPixel(Coordinate pixelCoordinate);
    public List<Pixel> GetPixels();
    public void AddPixel(Pixel pixel);
    public void UpdatePixel(Pixel pixel);
    public bool IsValidPlacement(Coordinate pixelCoordinate, User user);
}