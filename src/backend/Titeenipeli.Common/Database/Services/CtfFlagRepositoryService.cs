using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.Common.Database.Services;

public class CtfFlagRepositoryService(ApiDbContext dbContext) : RepositoryService(dbContext), ICtfFlagRepositoryService
{
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