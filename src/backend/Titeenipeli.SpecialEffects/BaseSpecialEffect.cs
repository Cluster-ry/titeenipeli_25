using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public abstract class BaseSpecialEffect : ISpecialEffect
{
    public abstract String Description { get; }
    public bool Directed { get; } = true;


    protected abstract byte[,] Template { get; }
    protected abstract Coordinate Origin { get; }



    public virtual List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        List<Coordinate> pixelsToPlace = [];

        for (int y = 0; y < Template.GetLength(0); y++)
        {
            for (int x = 0; x < Template.GetLength(1); x++)
            {
                if (Template[y, x] == 1)
                {
                    pixelsToPlace.Add(new Coordinate(x, y) - Origin + location);
                }
            }
        }

        return pixelsToPlace;
    }
}