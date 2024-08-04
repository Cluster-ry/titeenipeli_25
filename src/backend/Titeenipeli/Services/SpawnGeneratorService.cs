using Titeenipeli.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Services.RepositoryServices.Interfaces;

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
        Coordinate spawnCoordinate = AddOffset(_spawnGenerator.GetSpawnPoint(guildName));

        while (_mapRepositoryService.IsSpawn(spawnCoordinate))
        {
            spawnCoordinate = AddOffset(_spawnGenerator.GetSpawnPoint(guildName));
        }

        return spawnCoordinate;
    }

    private Coordinate AddOffset(Coordinate coordinate)
    {
        return new Coordinate
        {
            X = _mapCenter.X + coordinate.X,
            Y = _mapCenter.Y + coordinate.Y
        };
    }
}