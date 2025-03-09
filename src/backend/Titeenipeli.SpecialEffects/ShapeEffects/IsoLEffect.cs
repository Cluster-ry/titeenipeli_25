using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public class IsoLEffect : BaseSpecialEffect
{
    public override string Description { get; } = "Ota siitä L";

    // Original template before formating can be found in M-Files-logo-template
    protected override byte[,] Template { get; } =
    {
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 0, 0, 0, 0, 0, 0, 0, 0
        },
        {
            1, 1, 1, 1, 1, 1, 1, 0, 0, 0
        },
        {
            1, 1, 1, 1, 1, 1, 1, 0, 0, 0
        }
    };

    public override List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        return base.HandleSpecialEffect(location, Direction.East);
    }

    protected override Coordinate Origin { get; } = new(0, 0);
}