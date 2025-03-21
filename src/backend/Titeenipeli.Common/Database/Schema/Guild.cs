using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Database.Schema;

public class Guild : Entity
{
    public required GuildName Name { get; init; }
    public long CumulativeScore { get; set; }
    public int CurrentScore { get; set; }
    public float BaseRateLimit { get; set; }
    public int FogOfWarDistance { get; set; }
    public int PixelBucketSize { get; set; }
    public float RateLimitPerPlayer { get; set; }
    [Column(TypeName = "jsonb")]
    [MaxLength(10024)]
    public string? ActiveCtfFlags { get; set; }
}