using JetBrains.Annotations;
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
    private const GuildEnum Clus = GuildEnum.Cluster;
    private const GuildEnum Algo = GuildEnum.Algo;
    private const GuildEnum TiKi = GuildEnum.Tietokilta;

    [Fact]
    public void TestPlacePixel_ShouldReturnMapWithOnePlacedPixelIfPixelPlacedOnEmptyMap()
    {
        var mapUpdater = new MapUpdater();

        var updatedMap = mapUpdater.PlacePixel(_BuildEmptyMap(10, 10), (7, 8), GuildEnum.Cluster);
        
        Assert.Equal(GuildEnum.Cluster, updatedMap.Pixels[8, 7].Owner);
    }

    [Fact]
    public void TestPlacePixel_ShouldFillSurroundedAreas_SimpleMap()
    {
        var mapUpdater = new MapUpdater();

        GuildEnum?[,] initialMap =
        {
            { Clus, Clus, Clus, Clus },
            { Clus, Algo, TiKi, Clus },
            { Clus, TiKi, Algo, null },
            { Clus, Clus, Clus, Clus },
        };
        var map = _BuildMapFromOwnerArray(initialMap);
        map.Pixels[1, 1].Type = PixelTypeEnum.Spawn;

        var updatedMap = mapUpdater.PlacePixel(map, (4, 3), GuildEnum.Cluster);

        foreach (var pixel in updatedMap.Pixels)
        {
            if (pixel.Owner is not null)
            {
                Assert.Equal(GuildEnum.Cluster, pixel.Owner);
            }
        }
    }
    
    [Fact]
    public void TestPlacePixel_ShouldFillSurroundedAreas_ComplexMap()
    {
        var mapUpdater = new MapUpdater();

        GuildEnum?[,] initialMap =
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
        var map = _BuildMapFromOwnerArray(initialMap);
        map.Pixels[1, 2].Type = PixelTypeEnum.Spawn;

        var updatedMap = mapUpdater.PlacePixel(map, (7, 8), GuildEnum.Cluster);

        foreach (var pixel in updatedMap.Pixels)
        {
            if (pixel.Owner is not null)
            {
                Assert.Equal(GuildEnum.Cluster, pixel.Owner);
            }
        }
    }

    private MapModel _BuildEmptyMap(int xSize = 100, int ySize = 100)
    {
        var map = new PixelModel[ySize + 2, xSize + 2];
        for (var x = 0; x < xSize + 2; x++)
        {
            map[0, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        for (var y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
            for (var x = 1; x < xSize + 1; x++)
            {
                map[y, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.Normal };
            }

            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        for (var x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        return new MapModel { Pixels = map };
    }

    private MapModel _BuildMapFromOwnerArray(GuildEnum?[,] owners)
    {
        var ySize = owners.GetUpperBound(0) + 1;
        var xSize = owners.GetUpperBound(1) + 1;
        var map = new PixelModel[ySize + 2, xSize + 2];
        for (var x = 0; x < xSize + 2; x++)
        {
            map[0, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        for (var y = 1; y < ySize + 1; y++)
        {
            map[y, 0] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
            for (var x = 1; x < xSize + 1; x++)
            {
                map[y, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.Normal, Owner = owners[y - 1, x - 1]};
            }

            map[y, xSize + 1] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        for (var x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        return new MapModel { Pixels = map };
    }
}