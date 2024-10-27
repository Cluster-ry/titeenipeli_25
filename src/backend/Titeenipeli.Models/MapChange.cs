using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public struct MapChange(Coordinate coordinate, User? oldOwner, User? newOwner)
{
    public Coordinate Coordinate = coordinate;
    public User? OldOwner = oldOwner;
    public User? NewOwner = newOwner;
}
