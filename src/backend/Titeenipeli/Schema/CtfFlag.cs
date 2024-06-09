using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Schema;

public class CtfFlag
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; init; }

    public required string Flag { get; init; }
}