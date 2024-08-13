using Titeenipeli.Enums;
using Titeenipeli.Models;
using Titeenipeli.Options;

namespace Titeenipeli.GameLogic;

public class SpawnGenerator
{
    private readonly GameOptions _gameOptions;
    private readonly Random _random;

    public SpawnGenerator(GameOptions gameOptions)
    {
        _gameOptions = gameOptions;
        _random = new Random();
    }

    public Coordinate GetSpawnPoint(GuildName guild)
    {
        Coordinate spawnAreaCenter = GetSpawnAreaCenter(guild,
            _gameOptions.SpawnAreaDistanceFromCenter,
            _gameOptions.SpawnAreasPerGuild);

        Coordinate spawnPointInCircle = GetRandomPointInCircle(_gameOptions.SpawnAreaRadius);

        int spawnX = spawnAreaCenter.X + spawnPointInCircle.X;
        int spawnY = spawnAreaCenter.Y + spawnPointInCircle.Y;

        return new Coordinate { X = spawnX, Y = spawnY };
    }

    private Coordinate GetSpawnAreaCenter(GuildName guild, int radius, int spawnAreasPerGuild)
    {
        int guildCount = Enum.GetNames(typeof(GuildName)).Length;
        int selectedSpawnArea = _random.Next(spawnAreasPerGuild);

        double angleOffset = 2 * Math.PI / (guildCount * spawnAreasPerGuild);
        double angle = angleOffset * guildCount * selectedSpawnArea + angleOffset * (int)guild;

        int x = (int)Math.Round(Math.Cos(angle) * radius);
        int y = (int)Math.Round(Math.Sin(angle) * radius);

        return new Coordinate { X = x, Y = y };
    }

    private Coordinate GetRandomPointInCircle(int radius)
    {
        double angle = 2 * Math.PI * _random.NextDouble();
        double u = _random.NextDouble() + _random.NextDouble();
        double distance = u > 1 ? 2 - u : u;

        double x = distance * Math.Cos(angle);
        double y = distance * Math.Sin(angle);

        return new Coordinate
        {
            X = (int)Math.Round(x * radius),
            Y = (int)Math.Round(y * radius)
        };
    }
}