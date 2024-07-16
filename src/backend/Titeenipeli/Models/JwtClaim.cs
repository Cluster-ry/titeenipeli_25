namespace Titeenipeli.Models;

public class JwtClaim
{
    public required int Id { get; init; }

    public required Coordinate CoordinateOffset { get; init; }

    // TODO: Switch guild id to string
    public required int? GuildId { get; init; }
}