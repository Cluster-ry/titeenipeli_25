using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public class GrpcMapChangeInput(Coordinate coordinate, User oldOwner, User newOwner)
{
    public Coordinate Coordinate = coordinate;
    public User? OldOwner = oldOwner;
    public User? NewOwner = newOwner;
}
