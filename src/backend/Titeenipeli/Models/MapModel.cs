using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public class MapModel
{
    private const int FogOfWarDistance = 2;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public PixelModel[][] Pixels { get; set; }
    
    private readonly int _height;
    private readonly int _width;
    private int _maxViewableX;
    private int _maxViewableY;

    private int _minViewableX;
    private int _minViewableY;

    public MapModel(IEnumerable<Pixel> pixels, int width, int height, User? user)
    {
        _width = width;
        _height = height;
        _minViewableX = width;
        _minViewableY = height;
        _maxViewableX = 0;
        _maxViewableY = 0;
        Pixels = new PixelModel[height][];
        for (int y = 0; y < height; y++) Pixels[y] = new PixelModel[width];

        foreach (Pixel pixel in pixels)
        {
            PixelModel mapPixel = new PixelModel
            {
                Type = PixelTypeEnum.Normal,
                Owner = (GuildEnum?)pixel.User?.Guild.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == user
            };

            Pixels[pixel.Y][pixel.X] = mapPixel;
        }
    }

    public void MarkSpawns(IEnumerable<User> users)
    {
        foreach (User user in users) Pixels[user.SpawnY][user.SpawnX].Type = PixelTypeEnum.Spawn;
    }

    public void CalculateFogOfWar()
    {
        PixelModel[][] fogOfWarMap = new PixelModel[Pixels.Length][];
        for (int y = 0; y < Pixels.Length; y++) fogOfWarMap[y] = new PixelModel[Pixels[0].Length];

        for (int y = 0; y < Pixels.Length; y++)
        for (int x = 0; x < Pixels[0].Length; x++)
            if (Pixels[y][x].OwnPixel)
            {
                fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, x, y);
            }

        Pixels = TrimMap(fogOfWarMap);
    }

    private PixelModel[][] MarkPixelsInFogOfWar(PixelModel[][] fogOfWarMap, int pixelX, int pixelY)
    {
        int minX = int.Clamp(pixelX - FogOfWarDistance, 0, _width - 1);
        int minY = int.Clamp(pixelY - FogOfWarDistance, 0, _width - 1);
        int maxX = int.Clamp(pixelX + FogOfWarDistance, 0, _width - 1);
        int maxY = int.Clamp(pixelY + FogOfWarDistance, 0, _width - 1);

        _minViewableX = int.Min(minX - 1, _minViewableX);
        _minViewableY = int.Min(minY - 1, _minViewableY);
        _maxViewableX = int.Max(maxX + 1, _maxViewableX);
        _maxViewableY = int.Max(maxY + 1, _maxViewableY);

        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
            fogOfWarMap[y][x] = Pixels[y][x];

        return fogOfWarMap;
    }

    private PixelModel[][] TrimMap(PixelModel[][] map)
    {
        return map.Where((_, i) => _minViewableY < i && i < _maxViewableY)
            .Select(row => row.Where((_, i) => _minViewableX < i && i < _maxViewableX).ToArray()).ToArray();
    }
}