namespace Titeenipeli.Models;

public class Coordinate
{
    public int X { get; init; }
    public int Y { get; init; }

    public static Coordinate operator +(Coordinate a, Coordinate b)
    {
        return new Coordinate { X = a.X + b.X, Y = a.Y + b.Y };
    }
}