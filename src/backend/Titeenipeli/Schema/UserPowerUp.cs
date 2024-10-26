namespace Titeenipeli.Schema;

public class UserPowerUp : Entity
{
    public required User User { get; init; }
    public required List<PowerUp> PowerUps { get; init; }
}