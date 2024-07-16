namespace Titeenipeli.Schema;

public class Guild : Entity
{
    public required int Color { get; init; }
    public int CumulativeScore { get; set; } = 0;
}