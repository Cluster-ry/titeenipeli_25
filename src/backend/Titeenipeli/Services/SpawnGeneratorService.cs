using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;
using Titeenipeli.GameLogic;
using Titeenipeli.Options;

namespace Titeenipeli.Services;

public class SpawnGeneratorService
{
    private readonly Coordinate _mapCenter;
    private readonly IMapRepositoryService _mapRepositoryService;
    private readonly SpawnGenerator _spawnGenerator;

    public SpawnGeneratorService(GameOptions gameOptions, IMapRepositoryService mapRepositoryService)
    {
        _mapRepositoryService = mapRepositoryService;
        _spawnGenerator = new SpawnGenerator(gameOptions);
        _mapCenter = new Coordinate { X = gameOptions.Width / 2, Y = gameOptions.Height / 2 };
    }

    public Coordinate GetSpawnPoint(GuildName guildName)
    {
        Coordinate spawnCoordinate;

        do
        {
            spawnCoordinate = _mapCenter + _spawnGenerator.GetSpawnPoint(guildName);
        } while (!_mapRepositoryService.IsValid(spawnCoordinate) || _mapRepositoryService.IsSpawn(spawnCoordinate));

        return spawnCoordinate;
    }
}