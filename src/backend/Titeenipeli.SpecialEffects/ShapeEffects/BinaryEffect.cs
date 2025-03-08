using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public class BinaryEffect : BaseSpecialEffect
{
    public override string Description { get; } = "01010100 01001001 01010100 01000101 01000101 01001110 01001001 01010100";

    protected override byte[,] Template { get; } =
    {
        {1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1},
        {1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
        {1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
        {1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
        {1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1},
        {1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1}
    };

    public override List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        return base.HandleSpecialEffect(location, Direction.East);
    }
    protected override Coordinate Origin { get; } = new(5, 5);
}