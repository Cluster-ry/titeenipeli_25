using System.ComponentModel.DataAnnotations.Schema;

namespace Titeenipeli.Models;

public class CtfFlag
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public required int Id { get; init; }

    public required string Flag { get; init; }
}