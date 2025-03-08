using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.InMemoryProvider.MapProvider;

public class MapProvider : IMapProvider
{
    private List<Pixel> _map = [];
    private readonly MapDatabaseWriterService _mapDatabaseWriterService;

    public MapProvider(MapDatabaseWriterService mapDatabaseWriterService)
    {
        _mapDatabaseWriterService = mapDatabaseWriterService;
    }

    public void Initialize(List<Pixel> pixels)
    {
        _map = pixels;
    }

    public Pixel? GetByCoordinate(Coordinate pixelCoordinate)
    {
        lock (_map)
        {
            return _map.Find(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);
        }
    }

    public List<Pixel> GetAll()
    {
        lock (_map)
        {
            return _map;
        }
    }

    public void Update(Pixel pixel)
    {
        Pixel? pixelToUpdate;

        lock (_map)
        {
            pixelToUpdate = _map.Find(mapPixel => mapPixel.X == pixel.X && mapPixel.Y == pixel.Y);

            if (pixelToUpdate == null)
            {
                return;
            }

            pixelToUpdate.User = pixel.User;
        }

        _mapDatabaseWriterService.PixelChannel.Writer.TryWrite(pixelToUpdate);
    }

    public bool IsValid(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return pixel != null;
    }

    public bool IsSpawn(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return pixel?.User != null && pixel.User.SpawnX == pixel.X && pixel.User.SpawnY == pixel.Y;
    }
}