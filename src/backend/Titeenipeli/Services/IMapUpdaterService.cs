using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IMapUpdaterService
{
    public Task PlacePixel(Coordinate pixelCoordinates, User newOwner);
}
