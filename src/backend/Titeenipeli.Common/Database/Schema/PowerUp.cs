using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Common.Database.Schema;

public class PowerUp : Entity
{
    public int PowerId { get; init; }
    public bool Directed { get; init; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(32)]
    public required string Name { get; init; }
}