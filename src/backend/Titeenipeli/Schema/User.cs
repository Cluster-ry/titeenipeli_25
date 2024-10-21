using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class User : Entity
{
    public required Guild? Guild { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Code { get; init; }

    public required int SpawnX { get; set; }
    public required int SpawnY { get; set; }
    public float PixelBucket { get; set; } = 10;


    #region Telegram
    [StringLength(64)]
    public required string TelegramId { get; init; }

    [StringLength(64)]
    public required string FirstName { get; init; }

    [StringLength(64)]
    public required string LastName { get; init; }

    [StringLength(64)]
    public required string Username { get; init; }

    [StringLength(64)]
    public required string PhotoUrl { get; init; }

    [StringLength(64)]
    public required string AuthDate { get; init; }

    [StringLength(64)]
    public required string Hash { get; init; }
    #endregion
}