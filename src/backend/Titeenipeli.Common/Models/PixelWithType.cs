using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Models;

public class PixelWithType
{
    public Coordinate Location { get; init; }
    public PixelType Type { get; set; }
    public User? Owner { get; set; }

}