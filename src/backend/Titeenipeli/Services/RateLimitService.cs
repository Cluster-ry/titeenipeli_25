using Titeenipeli.Schema;

namespace Titeenipeli.Services;

public class RateLimitService
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private TimeSpan _cooldown = TimeSpan.FromSeconds(10);

    public bool CanPlacePixel(User user)
    {
        return DateTime.UtcNow - user.LastPlacement > _cooldown;
    }

    public TimeSpan TimeBeforeNextPixel(User user)
    {
        return user.LastPlacement + _cooldown - DateTime.UtcNow;
    }

    public int PlaceablePixelCount(User user)
    {
        return user.PixelBucket;
    }
}