﻿using System.Collections.Generic;
using System.Text;
using Titeenipeli.Enums;
using Titeenipeli.Models;

namespace Titeenipeli.Tests.GameLogic;

using GuildPixel = (GuildName? guild, bool isSpawn);

public static class MapUtils
{
    private static readonly Dictionary<GuildName, int> _colourMappings = new Dictionary<GuildName, int>
    {
        { GuildName.Cluster, 31 },
        { GuildName.Tietokilta, 30 },
        { GuildName.Algo, 32 },
        { GuildName.OulunTietoteekkarit, 33 },
        { GuildName.TietoTeekkarikilta, 35 },
        { GuildName.Digit, 36 },
        { GuildName.Datateknologerna, 37 }
    };

    public static Map BuildMapFromOwnerMatrix(GuildPixel[,] owners)
    {
        int ySize = owners.GetUpperBound(0) + 1;
        int xSize = owners.GetUpperBound(1) + 1;

        PixelModel[,] map = BuildMapBorders(xSize, ySize);

        for (int y = 1; y < ySize + 1; y++)
        {
            for (int x = 1; x < xSize + 1; x++)
            {
                GuildPixel pixelData = owners[y - 1, x - 1];
                map[y, x] = new PixelModel
                {
                    OwnPixel = false,
                    Type = pixelData.isSpawn ? PixelType.Spawn : PixelType.Normal,
                    Owner = pixelData.guild
                };
            }
        }

        return new Map { Pixels = map, Height = ySize, Width = xSize };
    }

    public static Map BuildMapFromSpawnPointList(int size, (int x, int y, GuildName owner)[] spawnPoints)
    {
        PixelModel[,] map = BuildMapBorders(size, size);

        for (int y = 1; y < size + 1; y++)
        {
            for (int x = 1; x < size + 1; x++)
            {
                map[y, x] = new PixelModel
                {
                    OwnPixel = false,
                    Type = PixelType.Normal,
                    Owner = null
                };
            }
        }

        foreach ((int x, int y, GuildName owner) in spawnPoints)
            map[y, x] = new PixelModel { OwnPixel = false, Type = PixelType.Spawn, Owner = owner };

        return new Map { Pixels = map, Height = size, Width = size };
    }

    private static PixelModel[,] BuildMapBorders(int xSize, int ySize)
    {
        PixelModel[,] map = new PixelModel[ySize + 2, xSize + 2];
        for (int x = 0; x < xSize + 2; x++)
        {
            map[0, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        for (int y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        for (int x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        return map;
    }

    public static string MapAsColours(Map map)
    {
        int ySize = map.Pixels.GetUpperBound(0) + 1;
        int xSize = map.Pixels.GetUpperBound(1) + 1;
        StringBuilder builder = new StringBuilder();
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                GuildName? pixelOwner = map.Pixels[y, x].Owner;
                if (pixelOwner is null)
                {
                    builder.Append(' ');
                    continue;
                }

                // For some reason static analysis doesn't recognize the null guard just above ... oh well
                builder.Append($"\x1b[{_colourMappings[(GuildName)pixelOwner]}m\u2588\x1b[0m");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    public static string MapAsNumbers(Map map)
    {
        int ySize = map.Pixels.GetUpperBound(0) + 1;
        int xSize = map.Pixels.GetUpperBound(1) + 1;
        StringBuilder builder = new StringBuilder();
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                GuildName? pixelOwner = map.Pixels[y, x].Owner;
                if (pixelOwner is null)
                {
                    builder.Append(' ');
                    continue;
                }

                // For some reason static analysis doesn't recognize the null guard just above ... oh well
                builder.Append((int)pixelOwner);
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}