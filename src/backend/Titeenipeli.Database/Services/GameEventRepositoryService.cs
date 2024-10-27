using Titeenipeli.Database.Services.Interfaces;
using Titeenipeli.Schema;

namespace Titeenipeli.Database.Services;

public class GameEventRepositoryService : IGameEventRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public GameEventRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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