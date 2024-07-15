﻿using JetBrains.Annotations;
using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Models;
using Titeenipeli.Schema;
using Xunit;

namespace Titeenipeli.Tests.GameLogic;

[TestSubject(typeof(MapUpdater))]
public class MapUpdaterTest
{
    // Saving some shortened versions of guild enum to make constructing test maps easier
    // null is 4 chars, so we want 4 char variables for symmetry
    private const GuildName Clus = GuildName.Cluster;
    private const GuildName Algo = GuildName.Algo;
    private const GuildName TiKi = GuildName.Tietokilta;

    [Fact]
    public void TestPlacePixel_ShouldReturnMapWithOnePlacedPixelIfPixelPlacedOnEmptyMap()
    {
        var mapUpdater = new MapUpdater();

        var map = _BuildEmptyMap(10, 10);

        mapUpdater.PlacePixel(map, (7, 8), GuildName.Cluster);

        Assert.Equal(GuildName.Cluster, map.Pixels[8, 7].Owner);
    }

    [Fact]
    public void TestPlacePixel_ShouldFillSurroundedAreas_SimpleMap()
    {
        var mapUpdater = new MapUpdater();

        GuildName?[,] initialMap =
        {
            { Clus, Clus, Clus, Clus },
            { Clus, Algo, TiKi, Clus },
            { Clus, TiKi, Algo, null },
            { Clus, Clus, Clus, Clus },
        };
        var map = _BuildMapFromOwnerArray(initialMap);
        map.Pixels[1, 1].Type = PixelType.Spawn;

        mapUpdater.PlacePixel(map, (4, 3), GuildName.Cluster);

        foreach (var pixel in map.Pixels)
        {
            if (pixel.Owner is not null)
            {
                Assert.Equal(GuildName.Cluster, pixel.Owner);
            }
        }
    }

    [Fact]
    public void TestPlacePixel_ShouldFillSurroundedAreas_ComplexMap()
    {
        var mapUpdater = new MapUpdater();

        GuildName?[,] initialMapPixelOwners =
        {
            { null, Clus, Clus, Clus, null, null, null, Clus, Clus, Clus, null },
            { Clus, Clus, TiKi, Clus, Clus, null, Clus, Clus, Algo, Clus, Clus },
            { Clus, TiKi, TiKi, Clus, Clus, Clus, Clus, Algo, Clus, TiKi, Clus },
            { Clus, TiKi, null, null, null, Algo, Algo, Algo, TiKi, null, Clus },
            { Clus, Clus, null, Algo, Algo, Algo, TiKi, TiKi, TiKi, null, Clus },
            { null, Clus, TiKi, TiKi, Algo, null, null, TiKi, Clus, Clus, Clus },
            { null, Clus, Clus, Clus, Algo, Algo, null, Clus, Clus, null, null },
            { Clus, Clus, null, Clus, Clus, Clus, null, Clus, null, null, Clus },
        };
        var map = _BuildMapFromOwnerArray(initialMapPixelOwners);
        map.Pixels[1, 2].Type = PixelType.Spawn;

        mapUpdater.PlacePixel(map, (7, 8), GuildName.Cluster);

        foreach (var pixel in map.Pixels)
        {
            if (pixel.Owner is not null)
            {
                Assert.Equal(GuildName.Cluster, pixel.Owner);
            }
        }
    }

    private static Map _BuildEmptyMap(int xSize = 100, int ySize = 100)
    {
        return new Map { Pixels = _BuildEmptyMapPixels(xSize, ySize), Height = ySize, Width = xSize };
    }

    private static Map _BuildMapFromOwnerArray(GuildName?[,] owners)
    {
        var ySize = owners.GetUpperBound(0) + 1;
        var xSize = owners.GetUpperBound(1) + 1;
        var map = _BuildEmptyMapPixels(xSize, ySize);

        for (var y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
            for (var x = 1; x < xSize + 1; x++)
            {
                map[y, x] =
                    new PixelModel { OwnPixel = false, Type = PixelType.Normal, Owner = owners[y - 1, x - 1] };
            }

            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        return new Map { Pixels = map, Height = ySize, Width = xSize };
    }

    private static PixelModel[,] _BuildEmptyMapPixels(int xSize, int ySize)
    {
        var map = new PixelModel[ySize + 2, xSize + 2];
        for (var x = 0; x < xSize + 2; x++)
        {
            map[0, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        for (var y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
            for (var x = 1; x < xSize + 1; x++)
            {
                map[y, x] = new PixelModel { OwnPixel = false, Type = PixelType.Normal };
            }

            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        for (var x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelType.MapBorder };
        }

        return map;
    }
}