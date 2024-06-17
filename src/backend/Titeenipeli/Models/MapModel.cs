using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public class MapModel
{
    private const int FogOfWarDistance = 2;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public PixelModel[,] Pixels { get; set; }
    
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
        Pixels = new PixelModel[width, height];

        foreach (Pixel pixel in pixels)
        {
            PixelModel mapPixel = new PixelModel
            {
                Type = PixelTypeEnum.Normal,
                Owner = (GuildEnum?)pixel.User?.Guild.Color,
                // TODO: Verify owning status of pixel, this can be done when we get user information from JWT
                OwnPixel = pixel.User == user
            };

            Pixels[pixel.X, pixel.Y] = mapPixel;
        }
    }

    public void MarkSpawns(IEnumerable<User> users)
    {
        foreach (User user in users) Pixels[user.SpawnX, user.SpawnY].Type = PixelTypeEnum.Spawn;
    }

    public void CalculateFogOfWar()
    {
        PixelModel[,] fogOfWarMap = new PixelModel[_width, _height];

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
            if (Pixels[x, y].OwnPixel)
            {
                fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, y, y);
            }

        Pixels = TrimMap(fogOfWarMap);
    }

    private PixelModel[,] MarkPixelsInFogOfWar(PixelModel[,] fogOfWarMap, int pixelX, int pixelY)
    {
        int minX = int.Clamp(pixelX - FogOfWarDistance, 0, _width - 1);
        int minY = int.Clamp(pixelY - FogOfWarDistance, 0, _width - 1);
        int maxX = int.Clamp(pixelX + FogOfWarDistance, 0, _width - 1);
        int maxY = int.Clamp(pixelY + FogOfWarDistance, 0, _width - 1);

        _minViewableX = int.Min(minX - 1, _minViewableX);
        _minViewableY = int.Min(minY - 1, _minViewableY);
        _maxViewableX = int.Max(maxX + 1, _maxViewableX);
        _maxViewableY = int.Max(maxY + 1, _maxViewableY);

        for (int x = minY; x <= maxY; x++)
        for (int y = minX; y <= maxX; y++)
            fogOfWarMap[x, y] = Pixels[x, y];

        return fogOfWarMap;
    }

    private PixelModel[,] TrimMap(PixelModel[,] map)
    {
        PixelModel[,] trimmedMap =
            new PixelModel[_maxViewableX - (_minViewableX + 1), _maxViewableY - (_minViewableY + 1)];

        for (int x = 0, trimmedX = 0; x < _width; x++)
        {
            if (_minViewableX >= x || x >= _maxViewableX)
            {
                continue;
            }

            for (int y = 0, trimmedY = 0; y < _height; y++)
            {
                if (_minViewableY >= y || y >= _maxViewableY)
                {
                    continue;
                }

                trimmedMap[trimmedX, trimmedY] = map[x, y];
                trimmedY++;
            }

            trimmedX++;
        }

        return trimmedMap;
    }
}