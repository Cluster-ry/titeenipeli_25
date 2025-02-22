using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Common.Database.Schema;

public class GameEvent : Entity
{
    [Column(TypeName = "jsonb")]
    [MaxLength(1024)]
    public required string Event { get; init; }
}