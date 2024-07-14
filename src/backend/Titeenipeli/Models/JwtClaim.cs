namespace Titeenipeli.Models;

public class JwtClaim
{
    public required int Id { get; set; }

    public required Coordinate CoordinateOffset { get; set; }

    // TODO: Switch guild id to string
    public required int? GuildId { get; set; }
}