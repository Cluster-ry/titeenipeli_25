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

    public void Add(GameEvent gameEvent)
    {
        _dbContext.GameEvents.Add(gameEvent);
        _dbContext.SaveChanges();
    }
}