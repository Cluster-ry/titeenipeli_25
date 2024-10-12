using BenchmarkDotNet.Attributes;
using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Models;
using Titeenipeli.Tests.GameLogic;

namespace Titeenipeli.Benchmark;

using PixelPlacement = (int x, int y, GuildName owner);

[SimpleJob]
public class MapUpdateBenchmark
{
    private readonly PixelWithType[,] _map;
    private readonly PixelPlacement[] _moveSequence;
    private readonly MapUpdater _mapUpdater;

    public MapUpdateBenchmark()
    {
        PixelPlacement[] spawnPoints =
        [
            new PixelPlacement(1, 1, GuildName.Cluster),
            new PixelPlacement(10, 10, GuildName.Tietokilta),
            new PixelPlacement(1, 10, GuildName.Algo),
            new PixelPlacement(10, 1, GuildName.Digit),
        ];
        _map = MapUtils.BuildMapFromSpawnPointList(10, spawnPoints);
        _mapUpdater = new MapUpdater();

        // TODO look into looking this into a JSON file maybe?
        // then again, the idea of inlining this is to not impact the benchmark performance so mb this is the best
        // approach. At least we would need to read and parse the JSON before the benchmark happens
        _moveSequence =
        [
            new PixelPlacement(2, 1, GuildName.Cluster),
            new PixelPlacement(9, 10, GuildName.Tietokilta),
            new PixelPlacement(1, 9, GuildName.Algo),
            new PixelPlacement(10, 2, GuildName.Digit),

            new PixelPlacement(3, 1, GuildName.Cluster),
            new PixelPlacement(8, 10, GuildName.Tietokilta),
            new PixelPlacement(1, 8, GuildName.Algo),
            new PixelPlacement(10, 3, GuildName.Digit),

            new PixelPlacement(4, 1, GuildName.Cluster),
            new PixelPlacement(7, 10, GuildName.Tietokilta),
            new PixelPlacement(2, 10, GuildName.Algo),
            new PixelPlacement(9, 3, GuildName.Digit),

            new PixelPlacement(4, 2, GuildName.Cluster),
            new PixelPlacement(6, 10, GuildName.Tietokilta),
            new PixelPlacement(2, 8, GuildName.Algo),
            new PixelPlacement(9, 1, GuildName.Digit),

            new PixelPlacement(4, 3, GuildName.Cluster),
            new PixelPlacement(5, 10, GuildName.Tietokilta),
            new PixelPlacement(3, 8, GuildName.Algo),
            new PixelPlacement(8, 1, GuildName.Digit),

            new PixelPlacement(4, 4, GuildName.Cluster),
            new PixelPlacement(4, 10, GuildName.Tietokilta),
            new PixelPlacement(3, 9, GuildName.Algo),
            new PixelPlacement(7, 1, GuildName.Digit),

            new PixelPlacement(1, 2, GuildName.Cluster),
            new PixelPlacement(3, 10, GuildName.Tietokilta),
            new PixelPlacement(4, 9, GuildName.Algo),
            new PixelPlacement(7, 2, GuildName.Digit),

            new PixelPlacement(1, 3, GuildName.Cluster),
            new PixelPlacement(2, 10, GuildName.Tietokilta),
            new PixelPlacement(4, 10, GuildName.Algo),
            new PixelPlacement(8, 3, GuildName.Digit),

            new PixelPlacement(3, 4, GuildName.Cluster),
            new PixelPlacement(10, 9, GuildName.Tietokilta),
            new PixelPlacement(5, 9, GuildName.Algo),
            new PixelPlacement(6, 1, GuildName.Digit),

            new PixelPlacement(2, 4, GuildName.Cluster),
            new PixelPlacement(10, 8, GuildName.Tietokilta),
            new PixelPlacement(6, 9, GuildName.Algo),
            new PixelPlacement(4, 1, GuildName.Digit),

            new PixelPlacement(5, 2, GuildName.Cluster),
            new PixelPlacement(10, 7, GuildName.Tietokilta),
            new PixelPlacement(7, 9, GuildName.Algo),
            new PixelPlacement(3, 1, GuildName.Digit),

            new PixelPlacement(5, 1, GuildName.Cluster),
            new PixelPlacement(9, 7, GuildName.Tietokilta),
            new PixelPlacement(7, 10, GuildName.Algo),
            new PixelPlacement(8, 4, GuildName.Digit),

            new PixelPlacement(4, 5, GuildName.Cluster),
            new PixelPlacement(8, 7, GuildName.Tietokilta),
            new PixelPlacement(1, 7, GuildName.Algo),
            new PixelPlacement(8, 5, GuildName.Digit),

            new PixelPlacement(4, 6, GuildName.Cluster),
            new PixelPlacement(7, 7, GuildName.Tietokilta),
            new PixelPlacement(1, 6, GuildName.Algo),
            new PixelPlacement(7, 5, GuildName.Digit),

            new PixelPlacement(3, 6, GuildName.Cluster),
            new PixelPlacement(7, 8, GuildName.Tietokilta),
            new PixelPlacement(2, 6, GuildName.Algo),
            new PixelPlacement(6, 5, GuildName.Digit),

            new PixelPlacement(2, 5, GuildName.Cluster),
            new PixelPlacement(7, 9, GuildName.Tietokilta),
            new PixelPlacement(3, 7, GuildName.Algo),
            new PixelPlacement(6, 4, GuildName.Digit),

            new PixelPlacement(5, 6, GuildName.Cluster),
            new PixelPlacement(6, 7, GuildName.Tietokilta),
            new PixelPlacement(2, 10, GuildName.Algo),
            new PixelPlacement(6, 3, GuildName.Digit),

            new PixelPlacement(6, 6, GuildName.Cluster),
            new PixelPlacement(5, 7, GuildName.Tietokilta),
            new PixelPlacement(5, 8, GuildName.Algo),
            new PixelPlacement(6, 2, GuildName.Digit),

            new PixelPlacement(7, 6, GuildName.Cluster),
            new PixelPlacement(10, 6, GuildName.Tietokilta),
            new PixelPlacement(4, 7, GuildName.Algo),
            new PixelPlacement(9, 5, GuildName.Digit),

            new PixelPlacement(7, 5, GuildName.Cluster),
            new PixelPlacement(10, 5, GuildName.Tietokilta),
            new PixelPlacement(3, 10, GuildName.Algo),
            new PixelPlacement(10, 4, GuildName.Digit),

            new PixelPlacement(7, 4, GuildName.Cluster),
            new PixelPlacement(8, 6, GuildName.Tietokilta),
            new PixelPlacement(1, 5, GuildName.Algo),
            new PixelPlacement(9, 6, GuildName.Digit),

            new PixelPlacement(7, 3, GuildName.Cluster),
            new PixelPlacement(9, 5, GuildName.Tietokilta),
            new PixelPlacement(1, 4, GuildName.Algo),
            new PixelPlacement(8, 6, GuildName.Digit),

            new PixelPlacement(6, 2, GuildName.Cluster),
            new PixelPlacement(8, 5, GuildName.Tietokilta),
            new PixelPlacement(6, 8, GuildName.Algo),
            new PixelPlacement(5, 1, GuildName.Digit),

            new PixelPlacement(6, 1, GuildName.Cluster),
            new PixelPlacement(7, 6, GuildName.Tietokilta),
            new PixelPlacement(6, 7, GuildName.Algo),
            new PixelPlacement(7, 3, GuildName.Digit),
        ];
    }

    [Benchmark]
    public void PlacePixels()
    {
        foreach (var (x, y, owner) in _moveSequence)
        {
            _mapUpdater.PlacePixel(_map, new(x, y), MapUtils.GuildNameToUser(owner));
        }
    }
}