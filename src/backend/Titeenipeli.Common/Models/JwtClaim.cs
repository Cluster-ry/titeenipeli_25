using Titeenipeli.Common.Enums;

namespace Titeenipeli.Common.Models;

public class JwtClaim
{
    public required int Id { get; init; }

    public required Coordinate CoordinateOffset { get; init; }

    public required GuildName? GuildId { get; init; }
}