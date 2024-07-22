using JetBrains.Annotations;
using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Models;
using Xunit;
using FluentAssertions;

namespace Titeenipeli.Tests.GameLogic;

using GuildPixel = (GuildName? guild, bool isSpawn);
using NewPixel = (GuildName guild, (int x, int y) coordinates);

[TestSubject(typeof(MapUpdater))]
public class MapUpdaterTest
{
    // Saving some shortened versions of guild enum to make constructing test maps easier
    //
    // We're naming spawn pixels with all caps. This is considered inconsistent naming by the static analyzer
    // ReSharper disable InconsistentNaming
    private static readonly GuildPixel None = (null, false);
    
    private static readonly GuildPixel Clus = (GuildName.Cluster, false);
    private static readonly GuildPixel CLUS = (GuildName.Cluster, true);

    private static readonly GuildPixel Algo = (GuildName.Algo, false);
    private static readonly GuildPixel ALGO = (GuildName.Algo, true);

    private static readonly GuildPixel TiKi = (GuildName.Tietokilta, false);
    private static readonly GuildPixel TIKI = (GuildName.Tietokilta, true);
    
    // ReSharper restore InconsistentNaming

    public static TheoryData<GuildPixel[,], NewPixel, GuildPixel[,]> MapTestData
    {
        get
        {
            var data = new TheoryData<GuildPixel[,], NewPixel, GuildPixel[,]>
            {
                {
                    new [,]
                    {
                        { None, CLUS, Clus, Clus, None, None, None, Clus, Clus, Clus, None },
                        { Clus, Clus, TiKi, Clus, Clus, None, Clus, Clus, Algo, Clus, Clus },
                        { Clus, TiKi, TiKi, Clus, Clus, Clus, Clus, Algo, Clus, TiKi, Clus },
                        { Clus, TiKi, None, None, None, Algo, Algo, Algo, TiKi, None, Clus },
                        { Clus, Clus, None, Algo, Algo, Algo, TiKi, TiKi, TiKi, None, Clus },
                        { None, Clus, TiKi, TiKi, Algo, None, None, TiKi, Clus, Clus, Clus },
                        { None, Clus, Clus, Clus, Algo, Algo, None, Clus, Clus, None, None },
                        { Clus, Clus, None, Clus, Clus, Clus, None, Clus, None, None, Clus },
                    },
                    (GuildName.Cluster, (7, 8)),
                    new [,]
                    {
                        { None, CLUS, Clus, Clus, None, None, None, Clus, Clus, Clus, None },
                        { Clus, Clus, Clus, Clus, Clus, None, Clus, Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                        { None, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                        { None, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, None, None },
                        { Clus, Clus, None, Clus, Clus, Clus, Clus, Clus, None, None, Clus },
                    }
                },
                {
                    new [,]
                    {
                        { Clus, Clus, CLUS, Clus },
                        { Clus, Algo, TiKi, Clus },
                        { Clus, TiKi, Algo, None },
                        { Clus, Clus, Clus, Clus },
                    },
                    (GuildName.Cluster, (4, 3)),
                    new [,]
                    {
                        { Clus, Clus, CLUS, Clus },
                        { Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus },
                    }
                },
                {
                    new [,]
                    {
                        { Clus, Clus, CLUS, Clus },
                        { Clus, ALGO, TiKi, Clus },
                        { Clus, Algo, Algo, None },
                        { Clus, Clus, Clus, Clus },
                    },
                    (GuildName.Cluster, (4, 3)),
                    new [,]
                    {
                        { Clus, Clus, CLUS, Clus },
                        { Clus, ALGO, Clus, Clus },
                        { Clus, Clus, Clus, Clus },
                        { Clus, Clus, Clus, Clus },
                    }
                },
                {
                    new [,]
                    {
                        {None, None, TiKi, None},
                        {None, None, TiKi, TiKi},
                        {None, CLUS, TiKi, None},
                        {TIKI, TiKi, TiKi, TiKi}
                    },
                    (GuildName.Cluster, (2, 4)),
                    new [,]
                    {
                        {None, None, None, None},
                        {None, None, None, None},
                        {None, CLUS, None, None},
                        {TIKI, TiKi, None, None}
                    }
                }
                
            };
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(MapTestData))]
    public void TestPlacePixel_Parametrized(
        GuildPixel[,] initialMap,
        NewPixel placedPixel,
        GuildPixel[,] resultingMap)
    {
        var mapUpdater = new MapUpdater();
        var map = MapUtils.BuildMapFromOwnerMatrix(initialMap);

        mapUpdater.PlacePixel(map, placedPixel.coordinates, placedPixel.guild);

        for (var x = 0; x < resultingMap.GetUpperBound(1); x++)
        {
            for (var y = 0; y < resultingMap.GetUpperBound(0); y++)
            {
                map.Pixels[y + 1, x + 1].Owner.Should().Be(resultingMap[y, x].guild,
                    $"Expected pixel in [{x}, {y}] to be owned by {resultingMap[y, x].guild}, " +
                    $"but was owned by {map.Pixels[y + 1, x + 1].Owner}");
            }
        } 
    }

    
}