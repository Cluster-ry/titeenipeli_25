using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.InMemoryMapProvider;

public interface IMapProvider
{
    public void Initialize(List<Pixel> pixels);
    public Pixel? GetByCoordinate(Coordinate pixelCoordinate);
    public List<Pixel> GetAll();
    public void Update(Pixel pixel);
    public bool IsValid(Coordinate pixelCoordinate);
    public bool IsSpawn(Coordinate pixelCoordinate);
}