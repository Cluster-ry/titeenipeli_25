using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Titeenipeli.Common.Database.Schema;

public class User : Entity
{
    public required Guild Guild { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Code { get; init; }

    public required int SpawnX { get; set; }
    public required int SpawnY { get; set; }
    public float PixelBucket { get; set; } = 10;
    [StringLength(32)]
    public string? AuthenticationToken { get; set; }
    public DateTime? AuthenticationTokenExpiryTime { get; set; }

    public required List<PowerUp> Powerups { get; set; }

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
}