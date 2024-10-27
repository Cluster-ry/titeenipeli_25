using Titeenipeli.Inputs;

public sealed class PowerInput
{
    public required int Id { get; init; }
    public required PostPixelsInput Location { get; init; }
    public required Direction direction { get; init; }

}


public enum Direction
{
    Undefined = 0,
    North = 1,
    West = 2,
    South = 3,
    East = 4,
}