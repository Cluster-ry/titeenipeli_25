using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Results;

public class GetEventsResult
{
    public required List<Event> Events { get; set; }
}

public class Event
{
    public Coordinate Pixel { get; set; }
    public GuildName Guild { get; set; }
    public DateTime Timestamp { get; set; }
}