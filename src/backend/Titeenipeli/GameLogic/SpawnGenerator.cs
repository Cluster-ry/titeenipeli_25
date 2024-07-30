using Titeenipeli.Enums;
using Titeenipeli.Models;
using Titeenipeli.Options;

namespace Titeenipeli.GameLogic;

public class SpawnGenerator
{
    private readonly GameOptions _gameOptions;

    public SpawnGenerator(GameOptions gameOptions)
    {
        _gameOptions = gameOptions;
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

    private static Coordinate GetSpawnAreaCenter(GuildName guild, int radius, int spawnAreasPerGuild)
    {
        int guildCount = Enum.GetNames(typeof(GuildName)).Length;
        int selectedSpawnArea = new Random().Next(spawnAreasPerGuild);

        double angleOffset = 2 * Math.PI / (guildCount * spawnAreasPerGuild);
        double angle = angleOffset * guildCount * selectedSpawnArea + angleOffset * (int)guild;

        int x = (int)Math.Round(Math.Cos(angle) * radius);
        int y = (int)Math.Round(Math.Sin(angle) * radius);

        return new Coordinate { X = x, Y = y };
    }

    private static Coordinate GetRandomPointInCircle(int radius)
    {
        Random random = new Random();

        double angle = 2 * Math.PI * random.NextDouble();
        double u = random.NextDouble() + random.NextDouble();
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