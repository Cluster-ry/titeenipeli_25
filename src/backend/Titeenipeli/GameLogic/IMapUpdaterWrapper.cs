using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.GameLogic;

public interface IMapUpdaterWrapper
{
    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner);
}
