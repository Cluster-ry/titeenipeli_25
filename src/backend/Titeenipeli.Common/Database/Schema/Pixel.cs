namespace Titeenipeli.Common.Database.Schema;

public class Pixel : Entity
{
    public required int X { get; init; }
    public required int Y { get; init; }
    public User? User { get; set; }
}