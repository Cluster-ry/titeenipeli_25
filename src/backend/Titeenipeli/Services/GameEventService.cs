using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Services;

public class GameEventService : IGameEventService
{
    private readonly ApiDbContext _dbContext;

    public GameEventService(ApiDbContext dbContext)
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