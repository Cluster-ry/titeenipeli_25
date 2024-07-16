using Titeenipeli.Enums;

namespace Titeenipeli.Models;

public class PixelModel
{
    public PixelType Type { get; set; }
    public GuildName? Owner { get; set; }
    public bool OwnPixel { get; init; }
}