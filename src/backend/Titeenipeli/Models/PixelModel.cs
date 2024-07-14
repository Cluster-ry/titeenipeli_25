using Titeenipeli.Enums;

namespace Titeenipeli.Models;

public class PixelModel
{
    public PixelType Type { get; set; }
    public GuildNames? Owner { get; init; }
    public bool OwnPixel { get; init; }
}