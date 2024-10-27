using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class PowerUp : Entity
{
    [Column(TypeName = "VARCHAR")]
    [StringLength(32)]
    public required string Name { get; init; }
}