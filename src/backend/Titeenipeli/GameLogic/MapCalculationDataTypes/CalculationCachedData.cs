namespace Titeenipeli.GameLogic.MapCalculationDataTypes;

public record CalculationCachedData
{
    public required Dictionary<int, Node> Nodes { get; init; }
    public required int[,] NodeMap { get; init; }
    public required int NextNodeId { get; set; }
};