using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

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