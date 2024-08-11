using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public struct Coordinate
{
    public int X { get; set; }
    public int Y { get; set; }

    public static Coordinate operator +(Coordinate a, Coordinate b)
        => new()
        {
            X = a.X + b.X,
            Y = a.Y + b.Y,
        };

    public Coordinate ToSpawnRelativeCoordinate(User user)
    {
        return new Coordinate()
        {
            X = X - user.SpawnX,
            Y = Y - user.SpawnY
        };
    }
}