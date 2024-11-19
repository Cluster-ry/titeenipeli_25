namespace Titeenipeli.Inputs;

public sealed class PowerInput
{
    public required int Id { get; init; }
    public required PostPixelsInput Location { get; init; }
    public required DirectionEnum Direction { get; init; }

}


public enum DirectionEnum
{
    Undefined = 0,
    North = 1,
    West = 2,
    South = 3,
    East = 4,
}