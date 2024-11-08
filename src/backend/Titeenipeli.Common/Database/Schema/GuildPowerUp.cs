namespace Titeenipeli.Common.Database.Schema;

public class GuildPowerUp : Entity
{
    public required Guild Guild { get; init; }
    public required PowerUp PowerUp { get; init; }
}