using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;
using Titeenipeli.InMemoryMapProvider;
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
        } while (!_mapProvider.IsValid(spawnCoordinate) || _mapProvider.IsSpawn(spawnCoordinate));

        return spawnCoordinate;
    }
}