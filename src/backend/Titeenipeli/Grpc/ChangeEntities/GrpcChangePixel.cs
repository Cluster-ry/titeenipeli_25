using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public struct GrpcChangePixel(Coordinate coordinate, User? user)
{
    public Coordinate Coordinate = coordinate;
    public User? User = user;
}