using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Common.Models;

public struct MapChange(Coordinate coordinate, User? oldOwner, User? newOwner)
{
    public Coordinate Coordinate = coordinate;
    public User? OldOwner = oldOwner;
    public User? NewOwner = newOwner;
}
