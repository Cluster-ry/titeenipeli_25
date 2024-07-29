using Titeenipeli.Enums;
using Titeenipeli.Models;

namespace Titeenipeli.GameLogic;

public class SpawnGenerator
{
    private const int SpawnAreasPerGuild = 2;

    public static Coordinate GetSpawnPoint(GuildName guild)
    {
        Coordinate spawnAreaCenter = GetSpawnAreaCenter(guild, 100);
        Coordinate spawnPointInCircle = GetRandomPointInCircle(20);

        int spawnX = spawnAreaCenter.X + spawnPointInCircle.X;
        int spawnY = spawnAreaCenter.Y + spawnPointInCircle.Y;

        return new Coordinate { X = spawnX, Y = spawnY };
    }

    private static Coordinate GetSpawnAreaCenter(GuildName guild, int radius)
    {
        int guildCount = Enum.GetNames(typeof(GuildName)).Length;
        int selectedSpawnArea = new Random().Next(SpawnAreasPerGuild);
        double angleOffset = 2 * Math.PI / (guildCount * SpawnAreasPerGuild);
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
        double r = u > 1 ? 2 - u : u;
        double x = r * Math.Cos(angle);
        double y = r * Math.Sin(angle);

        return new Coordinate
        {
            X = (int)Math.Round(x * radius),
            Y = (int)Math.Round(y * radius)
        };
    }
}