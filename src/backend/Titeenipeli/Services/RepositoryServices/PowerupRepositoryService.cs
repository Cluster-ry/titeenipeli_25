using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices.Interfaces;

namespace Titeenipeli.Services.RepositoryServices;

public sealed class PowerupRepositoryService : IPowerupRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public PowerupRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(PowerUp entity)
    {
        _dbContext.Add(entity);
        _dbContext.SaveChanges();
    }

    public List<PowerUp> GetAll()
    {
        return _dbContext.PowerUps.ToList();
    }

    public PowerUp? GetById(int id)
    {
        return _dbContext.PowerUps.Find(id);
    }

    public IReadOnlyCollection<PowerUp>? UserPowers(int userid)
    {
        return _dbContext.UserPowerUps.Find(userid)?.PowerUps;
    }
}


