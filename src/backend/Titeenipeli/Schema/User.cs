using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class User : Entity
{
    public required Guild? Guild { get; set; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Code { get; init; }

    public required int SpawnX { get; init; }
    public required int SpawnY { get; init; }


    #region Telegram

    [StringLength(64)]
    public string? TelegramId { get; init; }

    [StringLength(64)]
    public string? FirstName { get; init; }

    [StringLength(64)]
    public string? LastName { get; init; }

    [StringLength(64)]
    public string? Username { get; init; }

    [StringLength(64)]
    public string? PhotoUrl { get; init; }

    [StringLength(64)]
    public string? AuthDate { get; init; }

    [StringLength(64)]
    public string? Hash { get; init; }

    #endregion
}