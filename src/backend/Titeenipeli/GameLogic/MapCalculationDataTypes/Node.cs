using Titeenipeli.Enums;

namespace Titeenipeli.GameLogic.MapCalculationDataTypes;

public record Node
{
    public required GuildName? Guild { get; init; }
    public HashSet<int> Neighbours { get; init; } = [];
    public HashSet<(int x, int y)> Pixels { get; } = [];
    public bool HasSpawn { get; set; }
}