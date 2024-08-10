using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Grpc.ChangeEntities;

public class GrpcMapChangesInput
{
    public Dictionary<Coordinate, GrpcChangePixel> OldPixels;
    public List<GrpcMapChangeInput> Changes;
    public Dictionary<Coordinate, GrpcChangePixel> NewPixels;

    public GrpcMapChangesInput(Dictionary<Coordinate, GrpcChangePixel> oldPixels, List<GrpcMapChangeInput> changes)
    {
        OldPixels = oldPixels;
        Changes = changes;
        NewPixels = ComputeNewPixels();
    }

    private Dictionary<Coordinate, GrpcChangePixel> ComputeNewPixels()
    {
        Dictionary<Coordinate, GrpcChangePixel> newPixels = CloneOldPixels();
        foreach (var change in Changes)
        {
            newPixels[change.Coordinate] = new GrpcChangePixel(change.Coordinate, change.NewOwner);
        }
        return newPixels;
    }

    private Dictionary<Coordinate, GrpcChangePixel> CloneOldPixels()
    {
        Dictionary<Coordinate, GrpcChangePixel> newPixels = new();
        foreach (var oldPixel in OldPixels)
        {
            newPixels.Add(oldPixel.Key, oldPixel.Value);
        }
        return newPixels;
    }
}
