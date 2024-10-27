using System.Runtime.Serialization;

namespace Titeenipeli.Common.Models;

[DataContract]
public class Map
{
    public required PixelModel[,] Pixels { get; init; }

    public required int Height { get; init; }
    public required int Width { get; init; }

    public int MinViewableX { get; set; } = int.MaxValue;
    public int MaxViewableX { get; set; } = int.MinValue;
    public int MinViewableY { get; set; } = int.MaxValue;
    public int MaxViewableY { get; set; } = int.MinValue;
}