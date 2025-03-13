using SixLabors.ImageSharp;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.VideoGenerator;

public static class GuildColor
{
    public static Color GetGuildColor(GuildName guild)
    {
        return guild switch
        {
            GuildName.Nobody => Color.FromRgba(0, 0, 0, 0),
            GuildName.Tietokilta => Color.FromRgba(255, 98, 0, 127),
            GuildName.Algo => Color.FromRgba(191, 255, 0, 127),
            GuildName.Cluster => Color.FromRgba(255, 0, 0, 127),
            GuildName.OulunTietoteekkarit => Color.FromRgba(111, 255, 0, 127),
            GuildName.TietoTeekkarikilta => Color.FromRgba(0, 255, 149, 127),
            GuildName.Digit => Color.FromRgba(0, 255, 247, 127),
            GuildName.Sosa => Color.FromRgba(0, 0, 255, 127),
            GuildName.Date => Color.FromRgba(170, 0, 255, 127),
            GuildName.Tutti => Color.FromRgba(255, 0, 204, 127),
            _ => throw new ArgumentOutOfRangeException(nameof(guild), guild, null)
        };
    }
}