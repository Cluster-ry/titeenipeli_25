using Titeenipeli.Enums;
using Titeenipeli.Models;

namespace Titeenipeli.GameLogic.MapCalculationDataTypes;

public record Node
{
    public required GuildName? Guild { get; init; }
    public HashSet<int> Neighbours { get; } = [];
    public HashSet<Coordinate> Pixels { get; } = [];
    public bool HasSpawn { get; set; }
}