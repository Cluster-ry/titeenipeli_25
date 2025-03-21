using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Common.Database.Schema;

public class CtfFlag : Entity
{
    [Column(TypeName = "VARCHAR")]
    [StringLength(64)]
    public required string Token { get; init; }
    public PowerUp? Powerup { get; init; }
    public float BaserateMultiplier { get; init; }
    public int FogOfWarIncrease { get; init; }
    public int BucketSizeIncrease { get; init; }
}