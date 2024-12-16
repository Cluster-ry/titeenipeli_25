using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public interface IMapUpdaterService
{
    public Task PlacePixel(IMapRepositoryService mapRepositoryService,
                           IUserRepositoryService userRepositoryService,
                           Coordinate pixelCoordinates, User newOwner);
}
