using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class CtfFlag : Entity
{
    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Flag { get; init; }
}