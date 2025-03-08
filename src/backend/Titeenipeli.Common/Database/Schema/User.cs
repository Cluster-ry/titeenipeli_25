using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Common.Database.Schema;

public class User : Entity
{
    public required Guild Guild { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Code { get; init; }

    public required int SpawnX { get; set; }
    public required int SpawnY { get; set; }
    public float PixelBucket { get; set; }
    [StringLength(32)]
    public string? AuthenticationToken { get; set; }
    public DateTime? AuthenticationTokenExpiryTime { get; set; }

    public List<PowerUp> PowerUps { get; init; } = [];

    public bool IsGod { get; init; } = false;

    #region Telegram
    [StringLength(64)]
    public required string TelegramId { get; init; }

    [StringLength(64)]
    public required string FirstName { get; init; }

    [StringLength(64)]
    public required string LastName { get; init; }

    [StringLength(64)]
    public required string Username { get; init; }
    #endregion

    public User Clone()
    {
        return new User
        {
            Id = Id,
            Guild = Guild,
            Code = Code,
            SpawnX = SpawnX,
            SpawnY = SpawnY,
            PixelBucket = PixelBucket,
            AuthenticationToken = AuthenticationToken,
            AuthenticationTokenExpiryTime = AuthenticationTokenExpiryTime,
            PowerUps = PowerUps,
            IsGod = IsGod,

            TelegramId = TelegramId,
            FirstName = FirstName,
            LastName = LastName,
            Username = Username
        };
    }
}