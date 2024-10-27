using Microsoft.EntityFrameworkCore;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Common.Database.Services;
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
}


