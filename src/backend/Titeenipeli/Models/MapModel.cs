using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Titeenipeli.Models;

[DataContract]
public class MapModel
{
    // ReSharper disable once MemberCanBePrivate.Global
    [JsonProperty("pixels")] public required PixelModel[,] Pixels { get; set; }

    public required int Height { get; init; }
    public required int Width { get; init; }

    public int MinViewableX { get; set; } = int.MaxValue;
    public int MaxViewableX { get; set; } = int.MinValue;
    public int MinViewableY { get; set; } = int.MaxValue;
    public int MaxViewableY { get; set; } = int.MinValue;
}