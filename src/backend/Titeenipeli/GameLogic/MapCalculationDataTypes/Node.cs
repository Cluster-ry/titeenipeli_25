using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.GameLogic.MapCalculationDataTypes;

public record Node
{
    public required GuildName? Guild { get; init; }
    public HashSet<int> Neighbours { get; } = [];
    public HashSet<Coordinate> Pixels { get; } = [];
    public bool HasSpawn { get; set; }
}