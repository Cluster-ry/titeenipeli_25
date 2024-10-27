using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Results;

public class GameStateResults
{
    public required PixelBucket PixelBucket { get; init; }

    public required List<Score> Scores { get; init; }
}

public class PixelBucket
{
    public required int Amount { get; init; }
    public required int MaxAmount { get; init; }
    public required float IncreasePerMinute { get; init; }
}

public class Score
{
    public required GuildName Guild { get; init; }
    public required int Amount { get; init; }
}