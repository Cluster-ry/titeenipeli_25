using Titeenipeli.Enums;

namespace Titeenipeli.GameLogic.MapCalculationDataTypes;

public record Node
{
    public required GuildEnum? guild { get; init; }
    public HashSet<int> neighbours { get; } = [];
    public HashSet<(int, int)> pixels { get; } = [];
    public bool hasSpawn { get; set; }
}