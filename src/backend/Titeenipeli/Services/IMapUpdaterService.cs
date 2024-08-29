using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services;

public interface IMapUpdaterWrapper
{
    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner);
}
