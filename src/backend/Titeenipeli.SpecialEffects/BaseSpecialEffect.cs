using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public abstract class BaseSpecialEffect : ISpecialEffect
{
    public abstract byte[,] Template { get; }
    public abstract Coordinate Origin { get; }


    public virtual List<Coordinate> HandleSpecialEffect(Coordinate location)
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