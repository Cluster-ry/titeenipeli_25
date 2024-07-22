using Titeenipeli.Enums;
using Titeenipeli.Models;

namespace Titeenipeli.Tests.GameLogic;

using GuildPixel = (GuildName? guild, bool isSpawn);

public static class MapUtils
{
    public static Map BuildMapFromOwnerMatrix(GuildPixel[,] owners)
    {
        var ySize = owners.GetUpperBound(0) + 1;
        var xSize = owners.GetUpperBound(1) + 1;

        var map = BuildMapBorders(xSize, ySize);

        for (var y = 1; y < ySize + 1; y++)
        {
            for (var x = 1; x < xSize + 1; x++)
            {
                var pixelData = owners[y - 1, x - 1];
                map[y, x] = new PixelModel
                {
                    OwnPixel = false,
                    Type =  pixelData.isSpawn ? PixelType.Spawn : PixelType.Normal,
                    Owner = pixelData.guild
                };
            }
        }

        return new Map { Pixels = map, Height = ySize, Width = xSize };
    }

    public static Map BuildMapFromSpawnPointList(int size, (int x, int y, GuildName owner)[] spawnPoints)
    {
        var map = BuildMapBorders(size, size);
        
        for (var y = 1; y < size + 1; y++)
        {
            for (var x = 1; x < size + 1; x++)
            {
                map[y, x] = new PixelModel
                {
                    OwnPixel = false,
                    Type =  PixelType.Normal,
                    Owner = null
                };
            }
        }

        foreach (var (x, y, owner) in spawnPoints)
        {
            map[y, x] = new PixelModel { OwnPixel = false, Type = PixelType.Spawn, Owner = owner };
        }

        return new Map { Pixels = map, Height = size, Width = size };
    }

    private static PixelModel[,] BuildMapBorders(int xSize, int ySize)
    {
        var map = new PixelModel[ySize + 2, xSize + 2];
        for (var x = 0; x < xSize + 2; x++)
        {
            map[0, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }
        
        for (var y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }
        
        for (var x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        return map;
     }
}