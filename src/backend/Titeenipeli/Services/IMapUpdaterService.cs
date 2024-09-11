using Titeenipeli.Models;
using Titeenipeli.Schema;

namespace Titeenipeli.Services;

public interface IMapUpdaterService
{
    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner);
}
