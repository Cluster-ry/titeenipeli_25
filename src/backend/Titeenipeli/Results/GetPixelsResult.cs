using Titeenipeli.Models;

namespace Titeenipeli.Results;

public class GetPixelsResult
{
    public Coordinate? PlayerSpawn { get; set; }
    public required PixelModel[,] Pixels { get; set; }
}