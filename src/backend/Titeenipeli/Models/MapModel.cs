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

    public MapModel(IEnumerable<Pixel> pixels, int width, int height, User? user)
    {
        _width = width;
        _height = height;
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

        Pixels = fogOfWarMap;
    }

    private PixelModel[][] MarkPixelsInFogOfWar(PixelModel[][] fogOfWarMap, int x, int y)
    {
        int minX = x - FogOfWarDistance > 0 ? x - FogOfWarDistance : 0;
        int minY = y - FogOfWarDistance > 0 ? y - FogOfWarDistance : 0;
        int maxX = x + FogOfWarDistance < _width ? x + FogOfWarDistance : _width - 1;
        int maxY = y + FogOfWarDistance < _height ? y + FogOfWarDistance : _height - 1;
        for (int i = minY; i <= maxY; i++)
        {
            for (int j = minX; j <= maxX; j++)
            {
                fogOfWarMap[i][j] = Pixels[i][j];
            }
        }
        return fogOfWarMap;
    }
}
