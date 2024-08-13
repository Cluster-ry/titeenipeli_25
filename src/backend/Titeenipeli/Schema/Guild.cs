using System.ComponentModel.DataAnnotations;
using Titeenipeli.Enums;

namespace Titeenipeli.Schema;

public class Guild : Entity
{
    [StringLength(64)]
    public required string Name { get; init; }

    public required GuildName NameId { get; init; }
    public required int Color { get; init; }
    public int CumulativeScore { get; set; }
}