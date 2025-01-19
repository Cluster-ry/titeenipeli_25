using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public abstract class BaseSpecialEffect : ISpecialEffect
{
    public abstract byte[,] Template { get; }
    public abstract Coordinate Origin { get; }


    public virtual List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        List<Coordinate> pixelsToPlace = [];

        for (int y = 0; y < Template.GetLength(0); y++)
        {
            for (int x = 0; x < Template.GetLength(1); x++)
            {
                if (Template[y, x] == 1)
                {
                    var directionalPixel = NewDirectionalPixel(new Coordinate(x, y), location, direction);
                    pixelsToPlace.Add(directionalPixel);
                }
            }
        }

        return pixelsToPlace;
    }

    private Coordinate NewDirectionalPixel(Coordinate iterationCoordinate, Coordinate location, Direction direction)
    {
        switch (direction)
        {
            case Direction.Undefined:
            case Direction.East:
                return iterationCoordinate - Origin + location;
            case Direction.West:
                var westPixel = iterationCoordinate - Origin + location;
                westPixel.X = -westPixel.X;
                return westPixel;
            case Direction.North:
                var northPixelX = iterationCoordinate.Y - Origin.Y + location.X;
                var northPixelY = iterationCoordinate.X - Origin.X + location.Y;
                return new Coordinate(northPixelX, northPixelY);
            case Direction.South:
                var southPixelX = iterationCoordinate.Y - Origin.Y + location.X;
                var southPixelY = -(iterationCoordinate.X - Origin.X) + location.Y;
                return new Coordinate(southPixelX, southPixelY);
            default:
                return iterationCoordinate - Origin + location;
        }
    }
}