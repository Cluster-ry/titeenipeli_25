using Titeenipeli.Common.Models;
using Titeenipeli.Models;

namespace Titeenipeli.Grpc.ChangeEntities;

public class GrpcMapChangesInput
{
    public List<MapChange> Changes;
    public Dictionary<Coordinate, GrpcChangePixel> NewPixels;

    public GrpcMapChangesInput(Dictionary<Coordinate, GrpcChangePixel> newPixels, List<MapChange> changes)
    {
        NewPixels = newPixels;
        Changes = changes;
    }
}
