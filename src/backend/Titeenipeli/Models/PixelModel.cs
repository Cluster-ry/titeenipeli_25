using Titeenipeli.Enums;

namespace Titeenipeli.Models;

public class PixelModel
{
    public PixelTypeEnum Type { get; set; }
    public GuildEnum? Owner { get; set; }
    public bool OwnPixel { get; set; }
}