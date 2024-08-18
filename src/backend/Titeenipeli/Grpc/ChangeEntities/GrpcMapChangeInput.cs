using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public struct GrpcMapChangeInput(Coordinate coordinate, User? oldOwner, User? newOwner)
{
    public Coordinate Coordinate = coordinate;
    public User? OldOwner = oldOwner;
    public User? NewOwner = newOwner;
}
