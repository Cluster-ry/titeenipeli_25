using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;
using Titeenipeli.InMemoryProvider.MapProvider;
using Titeenipeli.Options;

namespace Titeenipeli.Services;

public class SpawnGeneratorService
{
    private readonly Coordinate _mapCenter;
    private readonly IMapProvider _mapProvider;
    private readonly SpawnGenerator _spawnGenerator;

    public SpawnGeneratorService(GameOptions gameOptions, IMapProvider mapProvider)
    {
        _mapProvider = mapProvider;
        _spawnGenerator = new SpawnGenerator(gameOptions);
        _mapCenter = new Coordinate { X = gameOptions.Width / 2, Y = gameOptions.Height / 2 };
    }

    public Coordinate GetSpawnPoint(GuildName guildName)
    {
        Coordinate spawnCoordinate;

        do
        {
            spawnCoordinate = _mapCenter + _spawnGenerator.GetSpawnPoint(guildName);
        } while (!_mapProvider.IsValid(spawnCoordinate) ||
                 _mapProvider.IsSpawn(spawnCoordinate) ||
                 !IsValidSpawnPlacement(spawnCoordinate));

        return spawnCoordinate;
    }

    private bool IsValidSpawnPlacement(Coordinate pixelCoordinate)
    {
        return !(from pixel in _mapProvider.GetAll()
                 where Math.Abs(pixel.X - pixelCoordinate.X) <= 1 &&
                       Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                       pixel.User?.SpawnX == pixel.X && pixel.User?.SpawnY == pixel.Y
                 select pixel).Any();
    }
}