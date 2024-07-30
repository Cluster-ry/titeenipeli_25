using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services;

public class SpawnGeneratorService
{
    private readonly IMapRepositoryService _mapRepositoryService;
    private readonly SpawnGenerator _spawnGenerator;

    public SpawnGeneratorService(GameOptions gameOptions, IMapRepositoryService mapRepositoryService)
    {
        _mapRepositoryService = mapRepositoryService;
        _spawnGenerator = new SpawnGenerator(gameOptions);
    }

    public Coordinate GetSpawnPoint(GuildName guildName)
    {
        Coordinate spawnCoordinate = _spawnGenerator.GetSpawnPoint(guildName);

        while (_mapRepositoryService.IsSpawn(spawnCoordinate))
        {
            spawnCoordinate = _spawnGenerator.GetSpawnPoint(guildName);
        }

        return spawnCoordinate;
    }
}