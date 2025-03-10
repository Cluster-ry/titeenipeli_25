using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Models;
using Titeenipeli.InMemoryProvider.MapProvider;

namespace Titeenipeli.Helpers;

public static class PixelPlacement
{
    public static bool IsValidPlacement(IMapProvider mapProvider, Coordinate pixelCoordinate, User user)
    {
        // Take neighboring pixels for the pixel the user is trying to set,
        // but remove cornering pixels and only return pixels belonging to
        // the user
        return (from pixel in mapProvider.GetAll()
                where Math.Abs(pixel.X - pixelCoordinate.X) <= 1 &&
                      Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      Math.Abs(pixel.X - pixelCoordinate.X) + Math.Abs(pixel.Y - pixelCoordinate.Y) <= 1 &&
                      pixel.User?.Id == user.Id
                select pixel).Any();
    }
}