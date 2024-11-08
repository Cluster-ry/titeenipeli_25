using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Grpc.ChangeEntities;

public struct GrpcChangePixel(Coordinate coordinate, User? user)
{
    public Coordinate Coordinate = coordinate;
    public User? User = user;
}