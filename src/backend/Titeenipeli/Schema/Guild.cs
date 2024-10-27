using Titeenipeli.Enums;

namespace Titeenipeli.Schema;

public class Guild : Entity
{
    public required GuildName Name { get; init; }
    public int CumulativeScore { get; set; }
    public int CurrentScore { get; set; }
    public float CurrentRateLimitIncreasePerMinutePerPlayer { get; set; }
}