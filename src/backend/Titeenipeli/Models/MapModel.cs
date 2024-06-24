using System.Runtime.Serialization;

namespace Titeenipeli.Models;

[DataContract]
public class MapModel
{
    [DataMember] public CoordinateModel? PlayerSpawn { get; set; }
    [DataMember] public required PixelModel[,] Pixels { get; set; }

    public required int Height { get; init; }
    public required int Width { get; init; }

    public int MinViewableX { get; set; } = int.MaxValue;
    public int MaxViewableX { get; set; } = int.MinValue;
    public int MinViewableY { get; set; } = int.MaxValue;
    public int MaxViewableY { get; set; } = int.MinValue;
}