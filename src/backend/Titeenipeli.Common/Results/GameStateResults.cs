using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Results;

public sealed class GameStateResults
{
    public required PixelBucket PixelBucket { get; init; }

    public required List<Score> Scores { get; init; }

    public required List<PowerUp> PowerUps { get; init; }
}

public sealed class PixelBucket
{
    public required int Amount { get; init; }
    public required int MaxAmount { get; init; }
    public required float IncreasePerMinute { get; init; }
}

public sealed class Score
{
    public required GuildName Guild { get; init; }
    public required int Amount { get; init; }
}

public sealed class PowerUp
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}