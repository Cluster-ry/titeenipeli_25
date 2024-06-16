namespace Titeenipeli.Models;

public class JwtClaimModel
{
    public required int Id { get; set; }


    public required CoordinateModel CoordinateOffset { get; set; }


    // TODO: Switch guild id to string
    public required int GuildId { get; set; }
}