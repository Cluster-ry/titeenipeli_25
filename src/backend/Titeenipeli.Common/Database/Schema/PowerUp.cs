using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Common.Database.Schema;

public class PowerUp : Entity
{
    [Column(TypeName = "VARCHAR")]
    [StringLength(32)]
    public required string Name { get; init; }
    [Column(TypeName = "VARCHAR")]
    [StringLength(256)]
    public required string Description { get; init; }
}