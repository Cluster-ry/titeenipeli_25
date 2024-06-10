using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class User : Entity
{
    public required Guild Guild { get; init; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Code { get; init; }

    public required int SpawnX { get; init; }
    public required int SpawnY { get; init; }
}