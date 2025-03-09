using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Results;

public class GetScoreResult
{
    public required List<GuildScore> Scores { get; set; }
}

public class GuildScore
{
    public GuildName Guild { get; set; }
    public int Score { get; set; }
}