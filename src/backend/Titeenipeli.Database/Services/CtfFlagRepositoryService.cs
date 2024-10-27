using Titeenipeli.Database.Services.Interfaces;
using Titeenipeli.Schema;

namespace Titeenipeli.Database.Services;

public class CtfFlagRepositoryService : ICtfFlagRepositoryService
{
    private readonly ApiDbContext _dbContext;

    public CtfFlagRepositoryService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public CtfFlag? GetById(int id)
    {
        return _dbContext.CtfFlags.Find(id);
    }

    public List<CtfFlag> GetAll()
    {
        return _dbContext.CtfFlags.ToList();
    }

    public void Add(CtfFlag ctfFlag)
    {
        _dbContext.CtfFlags.Add(ctfFlag);
        _dbContext.SaveChanges();
    }

    public CtfFlag? GetByToken(string token)
    {
        return _dbContext.CtfFlags.FirstOrDefault(flag => flag.Token == token);
    }
}