using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.Common.Database.Services;

public class GameEventRepositoryService(ApiDbContext dbContext) : RepositoryService(dbContext), IGameEventRepositoryService
{
    public GameEvent? GetById(int id)
    {
        return _dbContext.GameEvents.FirstOrDefault(gameEvent => gameEvent.Id == id);
    }

    public List<GameEvent> GetAll()
    {
        return _dbContext.GameEvents.ToList();
    }

    public void Add(GameEvent gameEvent)
    {
        _dbContext.GameEvents.Add(gameEvent);
        _dbContext.SaveChanges();
    }
}