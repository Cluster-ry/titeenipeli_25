using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IMapUpdaterService
{
    public Task<bool> PlacePixel(Coordinate pixelCoordinate, User newOwner);

    public Task<bool> PlacePixels(List<Coordinate> pixelCoordinates, User newOwner);

    public Task<User> PlaceSpawn(User user);
}
