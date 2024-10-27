using Titeenipeli.Common.Models;

namespace Titeenipeli.Results;

public class GetPixelsResult
{
    public required Coordinate PlayerSpawn { get; set; }
    public required PixelModel[,] Pixels { get; set; }
}