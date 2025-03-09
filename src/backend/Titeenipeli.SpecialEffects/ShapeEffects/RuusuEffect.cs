using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public class RuusuEffect : BaseSpecialEffect
{
    public override string Description { get; } = "Tuska viiltää rintaa";

    protected override byte[,] Template { get; } =
    {
        {0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 0, 0},
        {0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1},
        {0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 1},
        {0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0},
        {0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0},
        {0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0},
        {0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0},
        {0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0},
        {0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0},
        {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 1, 1, 1, 0},
        {0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0}

    };

    public override List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        return base.HandleSpecialEffect(location, Direction.East);
    }

    protected override Coordinate Origin { get; } = new(6, 5);
}