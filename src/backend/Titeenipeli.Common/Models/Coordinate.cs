namespace Titeenipeli.Common.Models;

public struct Coordinate(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    public static Coordinate operator +(Coordinate a, Coordinate b)
    {
        return new Coordinate { X = a.X + b.X, Y = a.Y + b.Y };
    }

    public static Coordinate operator -(Coordinate a, Coordinate b)
    {
        return new Coordinate { X = a.X - b.X, Y = a.Y - b.Y };
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}