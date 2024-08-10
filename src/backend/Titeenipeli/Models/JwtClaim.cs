using Titeenipeli.Enums;

namespace Titeenipeli.Models;

public class JwtClaim
{
    public required int Id { get; init; }

    public required Coordinate CoordinateOffset { get; init; }

    // TODO: Switch guild id to string
    public required GuildName? GuildId { get; init; }
}