using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Schema;

public class Guild : Entity
{
    public required GuildName Name { get; init; }
    public int CumulativeScore { get; set; }
    public int CurrentScore { get; set; }
    public float BaseRateLimit { get; set; }
    public int FogOfWarDistance { get; set; }
    public int PixelBucketSize { get; set; } = 10;
    public float RateLimitPerPlayer { get; set; }
    public List<CtfFlag> ActiveCtfFlags { get; init; } = [];
}