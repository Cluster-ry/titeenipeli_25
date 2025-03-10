using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Database.Services;

public class GameEventRepositoryService(ApiDbContext dbContext)
    : EntityRepositoryService(dbContext), IGameEventRepositoryService
{
    public GameEvent? GetById(int id)
    {
        return DbContext.GameEvents.FirstOrDefault(gameEvent => gameEvent.Id == id);
    }

    public List<GameEvent> GetAll()
    {
        return DbContext.GameEvents.ToList();
    }

    public void Add(GameEvent gameEvent)
    {
        DbContext.GameEvents.Add(gameEvent);
    }
}

public class PixelChangeEvent
{
    public Coordinate Pixel { get; set; }
    public required PixelChangeUser ToUser { get; set; }
    public required PixelChangeUser FromUser { get; set; }
}

public class PixelChangeUser
{
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public int? GuildId { get; set; }
    public GuildName? GuildName { get; set; }
}