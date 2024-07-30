namespace Titeenipeli.Options;

public class GameOptions
{
    public int Width { get; init; }
    public int Height { get; init; }
    public int FogOfWarDistance { get; init; }

    public int SpawnAreasPerGuild { get; init; }
    public int SpawnAreaDistanceFromCenter { get; init; }
    public int SpawnAreaRadius { get; init; }
}