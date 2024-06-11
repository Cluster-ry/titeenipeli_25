using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public class MapModel
{
    // ReSharper disable once MemberCanBePrivate.Global
    public PixelModel[][] Pixels { get; set; }

    private const int FogOfWarDistance = 2;
    private readonly int _width;
    private readonly int _height;

    private int _minViewableX;
    private int _minViewableY;
    private int _maxViewableX;
    private int _maxViewableY;

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

        for (int i = 0; i < Pixels.Length; i++)
        {
            for (int j = 0; j < Pixels[0].Length; j++)
                if (Pixels[i][j].OwnPixel)
                {
                    fogOfWarMap = MarkPixelsInFogOfWar(fogOfWarMap, j, i);
                }
        }

        Pixels = TrimMap(fogOfWarMap);
    }

    private PixelModel[][] MarkPixelsInFogOfWar(PixelModel[][] fogOfWarMap, int x, int y)
    {
        int minX = x - FogOfWarDistance > 0 ? x - FogOfWarDistance : 0;
        int minY = y - FogOfWarDistance > 0 ? y - FogOfWarDistance : 0;
        int maxX = x + FogOfWarDistance < _width ? x + FogOfWarDistance : _width - 1;
        int maxY = y + FogOfWarDistance < _height ? y + FogOfWarDistance : _height - 1;

        _minViewableX = int.Min(minX - 1, _minViewableX);
        _minViewableY = int.Min(minY - 1, _minViewableY);
        _maxViewableX = int.Max(maxX + 1, _maxViewableX);
        _maxViewableY = int.Max(maxY + 1, _maxViewableY);
        
        for (int i = minY; i <= maxY; i++)
        {
            for (int j = minX; j <= maxX; j++)
            {
                fogOfWarMap[i][j] = Pixels[i][j];
            }
        }
        return fogOfWarMap;
    }

    private PixelModel[][] TrimMap(PixelModel[][] map)
    {
        return map.Where((_, i) => _minViewableY < i && i < _maxViewableY)
            .Select(row => row.Where((_, i) => _minViewableX < i && i < _maxViewableX).ToArray()).ToArray();
    }
}
