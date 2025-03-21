﻿using System;
using System.Collections;
using FluentAssertions;
using NUnit.Framework;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;

namespace Titeenipeli.Tests.GameLogic;

using GuildPixel = (GuildName? guild, bool isSpawn);
using NewPixel = (GuildName guild, Coordinate coordinates);

[TestFixture]
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

    private static readonly GuildPixel Digi = (GuildName.Digit, false);

    private static readonly GuildPixel DIGI = (GuildName.Digit, true);
    // ReSharper restore InconsistentNaming

    [SetUp]
    public void BeforeEach()
    {
        _mapUpdater = new MapUpdater();
    }

    private MapUpdater _mapUpdater;

    [TestCaseSource(nameof(MapTestCases))]
    public void TestPlacePixel_Parametrized(
        GuildPixel[,] initialMap,
        NewPixel[] placedPixels,
        GuildPixel[,] resultingMap)
    {
        var map = MapUtils.BuildMapFromOwnerMatrix(initialMap);

        foreach (var (guild, coordinates) in placedPixels)
        {
            _mapUpdater.PlacePixel(map, coordinates, MapUtils.GuildNameToUser(guild));
            Console.WriteLine("--------------");
            Console.WriteLine(MapUtils.MapAsNumbers(map));
        }

        for (int x = 0; x < resultingMap.GetUpperBound(1); x++)
        {
            for (int y = 0; y < resultingMap.GetUpperBound(0); y++)
            {
                var guildName = map[x + 1, y + 1].Owner?.Guild?.Name;
                guildName.Should().Be(resultingMap[y, x].guild,
                    $"Expected pixel in [{x}, {y}] to be owned by {resultingMap[y, x].guild}, " +
                    $"but was owned by {map[x + 1, y + 1].Owner}\n\n" +
                    $"{MapUtils.MapAsColours(map)}");
            }
        }
    }

    public static IEnumerable MapTestCases
    {
        get
        {
            yield return new TestCaseData(
                new[,]
                {
                    { None, CLUS, Clus, Clus, None, None, None, Clus, Clus, Clus, None },
                    { Clus, Clus, TiKi, Clus, Clus, None, Clus, Clus, Algo, Clus, Clus },
                    { Clus, TiKi, TiKi, Clus, Clus, Clus, Clus, Algo, Clus, TiKi, Clus },
                    { Clus, TiKi, None, None, None, Algo, Algo, Algo, TiKi, None, Clus },
                    { Clus, Clus, None, Algo, Algo, Algo, TiKi, TiKi, TiKi, None, Clus },
                    { None, Clus, TiKi, TiKi, Algo, None, None, TiKi, Clus, Clus, Clus },
                    { None, Clus, Clus, Clus, Algo, Algo, None, Clus, Clus, None, None },
                    { Clus, Clus, None, Clus, Clus, Clus, None, Clus, None, None, Clus }
                },
                new[] { (GuildName.Cluster, new Coordinate(7, 8)) },
                new[,]
                {
                    { None, CLUS, Clus, Clus, None, None, None, Clus, Clus, Clus, None },
                    { Clus, Clus, Clus, Clus, Clus, None, Clus, Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                    { None, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus },
                    { None, Clus, Clus, Clus, Clus, Clus, Clus, Clus, Clus, None, None },
                    { Clus, Clus, None, Clus, Clus, Clus, Clus, Clus, None, None, Clus }
                }).SetName("Should fill encircled area (large map)");

            yield return new TestCaseData(
                new[,]
                {
                    { Clus, Clus, CLUS, Clus },
                    { Clus, Algo, TiKi, Clus },
                    { Clus, TiKi, Algo, None },
                    { Clus, Clus, Clus, Clus }
                },
                new[] { (GuildName.Cluster, new Coordinate(4, 3)) },
                new[,]
                {
                    { Clus, Clus, CLUS, Clus },
                    { Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus }
                }).SetName("Should fill encircled area (small map)");

            yield return new TestCaseData(
                new[,]
                {
                    { Clus, Clus, CLUS, Clus, Clus },
                    { Clus, ALGO, Clus, Clus, Clus },
                    { Clus, Algo, Clus, Clus, Clus },
                    { Clus, Algo, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus }
                },
                new[] { (GuildName.Cluster, new Coordinate(2, 3)) },
                new[,]
                {
                    { Clus, Clus, CLUS, Clus, Clus },
                    { Clus, ALGO, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus },
                    { Clus, Clus, Clus, Clus, Clus }
                }).SetName("Should fill cut cells");

            yield return new TestCaseData(
                new[,]
                {
                    { Clus, Clus, CLUS, Clus },
                    { Clus, ALGO, TiKi, Clus },
                    { Clus, Algo, Algo, None },
                    { Clus, Clus, Clus, Clus }
                },
                new[] { (GuildName.Cluster, new Coordinate(4, 3)) },
                new[,]
                {
                    { Clus, Clus, CLUS, Clus },
                    { Clus, ALGO, Clus, Clus },
                    { Clus, Algo, Algo, Clus },
                    { Clus, Clus, Clus, Clus }
                }).SetName("Should not fill spawn connected cells");

            yield return new TestCaseData(
                new[,]
                {
                    { None, None, TiKi, None },
                    { None, None, TiKi, TiKi },
                    { None, CLUS, TiKi, None },
                    { TIKI, TiKi, TiKi, TiKi }
                },
                new[] { (GuildName.Cluster, new Coordinate(2, 4)) },
                new[,]
                {
                    { None, None, None, None },
                    { None, None, None, None },
                    { None, CLUS, None, None },
                    { TIKI, TiKi, None, None }
                }).SetName("Should cut cells not connected to spawn cells");

            yield return new TestCaseData(
                new[,]
                {
                    { CLUS, None, None, None, None, None, None, None, None, DIGI },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { None, None, None, None, None, None, None, None, None, None },
                    { ALGO, None, None, None, None, None, None, None, None, TIKI }
                },
                new[]
                {
                    (GuildName.Cluster, new Coordinate(2, 1)),
                    (GuildName.Tietokilta, new Coordinate(9, 10)),
                    (GuildName.Algo, new Coordinate(1, 9)),
                    (GuildName.Digit, new Coordinate(10, 2)),

                    (GuildName.Cluster, new Coordinate(3, 1)),
                    (GuildName.Tietokilta, new Coordinate(8, 10)),
                    (GuildName.Algo, new Coordinate(1, 8)),
                    (GuildName.Digit, new Coordinate(10, 3)),

                    (GuildName.Cluster, new Coordinate(4, 1)),
                    (GuildName.Tietokilta, new Coordinate(7, 10)),
                    (GuildName.Algo, new Coordinate(2, 10)),
                    (GuildName.Digit, new Coordinate(9, 3)),

                    (GuildName.Cluster, new Coordinate(4, 2)),
                    (GuildName.Tietokilta, new Coordinate(6, 10)),
                    (GuildName.Algo, new Coordinate(2, 8)),
                    (GuildName.Digit, new Coordinate(9, 1)),

                    (GuildName.Cluster, new Coordinate(4, 3)),
                    (GuildName.Tietokilta, new Coordinate(5, 10)),
                    (GuildName.Algo, new Coordinate(3, 8)),
                    (GuildName.Digit, new Coordinate(8, 1)),

                    (GuildName.Cluster, new Coordinate(4, 4)),
                    (GuildName.Tietokilta, new Coordinate(4, 10)),
                    (GuildName.Algo, new Coordinate(3, 9)),
                    (GuildName.Digit, new Coordinate(7, 1)),

                    (GuildName.Cluster, new Coordinate(1, 2)),
                    (GuildName.Tietokilta, new Coordinate(3, 10)),
                    (GuildName.Algo, new Coordinate(4, 9)),
                    (GuildName.Digit, new Coordinate(7, 2)),

                    (GuildName.Cluster, new Coordinate(1, 3)),
                    (GuildName.Tietokilta, new Coordinate(2, 10)),
                    (GuildName.Algo, new Coordinate(4, 10)),
                    (GuildName.Digit, new Coordinate(8, 3)),

                    (GuildName.Cluster, new Coordinate(3, 4)),
                    (GuildName.Tietokilta, new Coordinate(10, 9)),
                    (GuildName.Algo, new Coordinate(5, 9)),
                    (GuildName.Digit, new Coordinate(6, 1)),

                    (GuildName.Cluster, new Coordinate(2, 4)),
                    (GuildName.Tietokilta, new Coordinate(10, 8)),
                    (GuildName.Algo, new Coordinate(6, 9)),
                    (GuildName.Digit, new Coordinate(5, 1)),

                    (GuildName.Cluster, new Coordinate(5, 2)),
                    (GuildName.Tietokilta, new Coordinate(10, 7)),
                    (GuildName.Algo, new Coordinate(7, 9)),
                    (GuildName.Digit, new Coordinate(4, 1)),

                    (GuildName.Cluster, new Coordinate(5, 1)),
                    (GuildName.Tietokilta, new Coordinate(9, 7)),
                    (GuildName.Algo, new Coordinate(7, 10)),
                    (GuildName.Digit, new Coordinate(8, 4)),

                    (GuildName.Cluster, new Coordinate(4, 5)),
                    (GuildName.Tietokilta, new Coordinate(8, 7)),
                    (GuildName.Algo, new Coordinate(1, 7)),
                    (GuildName.Digit, new Coordinate(8, 5)),

                    (GuildName.Cluster, new Coordinate(4, 6)),
                    (GuildName.Tietokilta, new Coordinate(7, 7)),
                    (GuildName.Algo, new Coordinate(1, 6)),
                    (GuildName.Digit, new Coordinate(7, 5)),

                    (GuildName.Cluster, new Coordinate(3, 6)),
                    (GuildName.Tietokilta, new Coordinate(7, 8)),
                    (GuildName.Algo, new Coordinate(2, 6)),
                    (GuildName.Digit, new Coordinate(6, 5)),

                    (GuildName.Cluster, new Coordinate(2, 5)),
                    (GuildName.Tietokilta, new Coordinate(7, 9)),
                    (GuildName.Algo, new Coordinate(3, 7)),
                    (GuildName.Digit, new Coordinate(6, 4)),

                    (GuildName.Cluster, new Coordinate(5, 6)),
                    (GuildName.Tietokilta, new Coordinate(6, 7)),
                    (GuildName.Algo, new Coordinate(2, 10)),
                    (GuildName.Digit, new Coordinate(6, 3)),

                    (GuildName.Cluster, new Coordinate(6, 6)),
                    (GuildName.Tietokilta, new Coordinate(5, 7)),
                    (GuildName.Algo, new Coordinate(5, 8)),
                    (GuildName.Digit, new Coordinate(6, 2)),

                    (GuildName.Cluster, new Coordinate(7, 6)),
                    (GuildName.Tietokilta, new Coordinate(10, 6)),
                    (GuildName.Algo, new Coordinate(4, 7)),
                    (GuildName.Digit, new Coordinate(9, 5)),

                    (GuildName.Cluster, new Coordinate(7, 5)),
                    (GuildName.Tietokilta, new Coordinate(10, 5)),
                    (GuildName.Algo, new Coordinate(3, 10)),
                    (GuildName.Digit, new Coordinate(10, 4)),

                    (GuildName.Cluster, new Coordinate(7, 4)),
                    (GuildName.Tietokilta, new Coordinate(8, 6)),
                    (GuildName.Algo, new Coordinate(1, 5)),
                    (GuildName.Digit, new Coordinate(9, 6)),

                    (GuildName.Cluster, new Coordinate(7, 3)),
                    (GuildName.Tietokilta, new Coordinate(9, 5)),
                    (GuildName.Algo, new Coordinate(1, 4)),
                    (GuildName.Digit, new Coordinate(8, 6)),

                    (GuildName.Cluster, new Coordinate(6, 2)),
                    (GuildName.Tietokilta, new Coordinate(8, 5)),
                    (GuildName.Algo, new Coordinate(6, 8)),
                    (GuildName.Digit, new Coordinate(5, 1)),

                    (GuildName.Cluster, new Coordinate(6, 1)),
                    (GuildName.Tietokilta, new Coordinate(7, 6)),
                    (GuildName.Algo, new Coordinate(6, 7)),
                    (GuildName.Digit, new Coordinate(7, 3))
                },
                new[,]
                {
                    { CLUS, Clus, Clus, None, None, Clus, Digi, Digi, Digi, DIGI },
                    { Clus, Clus, Clus, Clus, Clus, Clus, Digi, Digi, Digi, Digi },
                    { Clus, Clus, Clus, Clus, Clus, Clus, Digi, Digi, Digi, Digi },
                    { Algo, Clus, Clus, Clus, Clus, Clus, Clus, Digi, Digi, Digi },
                    { Algo, Clus, Clus, Clus, Clus, Clus, Clus, TiKi, TiKi, TiKi },
                    { Algo, Algo, Clus, Clus, Clus, Clus, TiKi, TiKi, TiKi, TiKi },
                    { Algo, Algo, Algo, Algo, None, Algo, TiKi, TiKi, TiKi, TiKi },
                    { Algo, Algo, Algo, Algo, Algo, Algo, TiKi, TiKi, TiKi, TiKi },
                    { Algo, Algo, Algo, Algo, Algo, Algo, TiKi, TiKi, TiKi, TiKi },
                    { ALGO, Algo, Algo, Algo, None, None, None, TiKi, TiKi, TIKI }
                }).SetName("Should have valid benchmark case");

            yield return new TestCaseData(
                new[,]
                {
                    { CLUS, Clus, None, None, DIGI },
                    { None, None, None, None, None },
                    { None, None, None, None, None },
                    { None, None, None, None, None },
                    { ALGO, None, None, None, TIKI }
                },
                new[] { (GuildName.Tietokilta, new Coordinate(4, 5)) },
                new[,]
                {
                    { CLUS, Clus, None, None, DIGI },
                    { None, None, None, None, None },
                    { None, None, None, None, None },
                    { None, None, None, None, None },
                    { ALGO, None, None, TiKi, TIKI }
                }).SetName("Should not fill non-encircled space on a sparse map");

            yield return new TestCaseData(
                new[,]
                {
                    { None, None, CLUS, None, None },
                    { None, None, Clus, None, None },
                    { None, Clus, Clus, None, None },
                    { None, None, None, None, None },
                    { None, None, None, None, ALGO }
                },
                new[] { (GuildName.Algo, new Coordinate(4, 5)) },
                new[,]
                {
                    { None, None, CLUS, None, None },
                    { None, None, Clus, None, None },
                    { None, Clus, Clus, None, None },
                    { None, None, None, None, None },
                    { None, None, None, Algo, ALGO }
                }).SetName("Should not cut J-shape when another guild places pixel");

            yield return new TestCaseData(
                new[,]
                {
                    { None, None, None, None, None },
                    { None, None, None, None, None },
                    { None, CLUS, Clus, Clus, None },
                    { None, None, None, None, None },
                    { None, None, None, None, None }
                },
                new[] { (GuildName.Cluster, new Coordinate(4, 2)) },
                new[,]
                {
                    { None, None, None, None, None },
                    { None, None, None, Clus, None },
                    { None, Clus, Clus, Clus, None },
                    { None, None, None, None, None },
                    { None, None, None, None, None }
                }).SetName("Should not cut reverse L-shape when L top is set last");
        }
    }
}