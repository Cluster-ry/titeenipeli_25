using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.SpecialEffects;

public sealed class TiteenikirvesEffect(int height, int width) : ISpecialEffect
{
    public List<Coordinate> HandleSpecialEffect(Coordinate location, Direction direction)
    {
        List<Coordinate> axeCoordinates = [];

        switch (direction)
        {
            case Direction.North or Direction.South:
                for (var y = 0; y < height; y++)
                {
                    //Axe cut is 3 pixel wide
                    axeCoordinates.Add(new Coordinate { X = location.X - 1, Y = y });
                    axeCoordinates.Add(new Coordinate { X = location.X, Y = y });
                    axeCoordinates.Add(new Coordinate { X = location.X + 1, Y = y });
                }
                break;
            case Direction.West or Direction.East:
                for (var x = 0; x < width; x++)
                {
                    //Axe cut is 3 pixel wide
                    axeCoordinates.Add(new Coordinate { X = x, Y = location.Y - 1 });
                    axeCoordinates.Add(new Coordinate { X = x, Y = location.Y });
                    axeCoordinates.Add(new Coordinate { X = x, Y = location.Y + 1 });
                }

                break;
        }

        return axeCoordinates;
    }
}