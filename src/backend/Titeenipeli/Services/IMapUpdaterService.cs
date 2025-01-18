using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IMapUpdaterService
{
    public Task<bool> PlacePixel(IUserRepositoryService userRepositoryService, Coordinate pixelCoordinate,
                                 User newOwner);

    public Task<User> PlaceSpawn(IUserRepositoryService userRepositoryService, User user);
}
