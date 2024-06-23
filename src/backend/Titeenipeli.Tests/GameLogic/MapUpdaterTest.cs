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
    [Fact]
    public void TestPlacePixel_ShouldReturnMapWithOnePlacedPixelIfPixelPlacedOnEmptyMap()
    {
        var mapUpdater = new MapUpdater();

        var updatedMap = mapUpdater.PlacePixel(_BuildEmptyMap(), (47, 58), GuildEnum.Cluster);
        
        Assert.Equal(updatedMap.Pixels[47, 58].Owner, GuildEnum.Cluster);
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

            map[y, xSize] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        for (var x = 0; x < xSize + 2; x++)
        {
            map[ySize + 1, x] = new PixelModel { OwnPixel = false, Type = PixelTypeEnum.MapBorder };
        }

        return new MapModel { Pixels = map };
    }
}