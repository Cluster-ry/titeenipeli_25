using Titeenipeli.Models;

namespace Titeenipeli.Grpc.ChangeEntities;

public class GrpcMapChangesInput
{
    public List<GrpcMapChangeInput> Changes;
    public Dictionary<Coordinate, GrpcChangePixel> NewPixels;

    public GrpcMapChangesInput(Dictionary<Coordinate, GrpcChangePixel> newPixels, List<GrpcMapChangeInput> changes)
    {
        NewPixels = newPixels;
        Changes = changes;
    }
}
