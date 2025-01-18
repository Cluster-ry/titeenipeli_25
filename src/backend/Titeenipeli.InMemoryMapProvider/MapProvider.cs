using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.InMemoryMapProvider;

public class MapProvider : IMapProvider
{
    private List<Pixel> _map = [];
    private readonly DatabaseWriterService _databaseWriterService;

    public MapProvider(DatabaseWriterService databaseWriterService)
    {
        _databaseWriterService = databaseWriterService;
    }

    public void Initialize(List<Pixel> pixels)
    {
        _map = pixels;
    }

    public Pixel? GetByCoordinate(Coordinate pixelCoordinate)
    {
        lock (_map)
        {
            return _map.FirstOrDefault(pixel => pixel.X == pixelCoordinate.X && pixel.Y == pixelCoordinate.Y);
        }
    }

    public List<Pixel> GetAll()
    {
        lock (_map)
        {
            return _map.ToList();
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

        _databaseWriterService.PixelChannel.Writer.TryWrite(pixelToUpdate);
    }

    public bool IsValid(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return pixel != null;
    }

    public bool IsSpawn(Coordinate pixelCoordinate)
    {
        var pixel = GetByCoordinate(pixelCoordinate);
        return !(pixel?.User == null || (pixel.User.SpawnX != pixel.X && pixel.User.SpawnY != pixel.Y));
    }
}