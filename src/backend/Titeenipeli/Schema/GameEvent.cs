using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class GameEvent : Entity
{
    public required User User { get; init; }

    [Column(TypeName = "jsonb")]
    [MaxLength(1024)]
    public required string Event { get; init; }
}