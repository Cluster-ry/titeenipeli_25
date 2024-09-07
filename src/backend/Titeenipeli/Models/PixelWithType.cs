using Titeenipeli.Enums;
using Titeenipeli.Schema;

namespace Titeenipeli.Models;

public class PixelWithType
{
    public Coordinate Location { get; init; }
    public PixelType Type { get; set; }
    public User? User { get; set; }

}