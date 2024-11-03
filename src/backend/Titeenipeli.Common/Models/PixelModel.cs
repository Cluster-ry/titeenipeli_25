using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Models;

public class PixelModel
{
    public PixelType Type { get; set; }
    public GuildName? Guild { get; set; }
    public int Owner { get; set; }
    public string? BackgroundGraphic { get; set; }
}