using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Models;

public class JwtClaim
{
    public required int Id { get; init; }

    public required Coordinate CoordinateOffset { get; init; }

    public required GuildName? GuildId { get; init; }
}